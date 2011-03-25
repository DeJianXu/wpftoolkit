﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Windows.Controls.Core.Utilities;

namespace Microsoft.Windows.Controls
{
    public class Calculator : Control
    {
        #region Members

        private ContentControl _buttonPanel;
        private readonly DispatcherTimer _timer;
        private Button _calculatorButton;

        private bool _showNewNumber = true;
        private decimal _previousValue;
        private Operation _lastOperation = Operation.None;

        #endregion //Members

        #region Enumerations

        public enum CalculatorButtonType
        {
            Add,
            Back,
            Cancel,
            Clear,
            Decimal,
            Divide,
            Eight,
            Equal,
            Five,
            Four,
            Fraction,
            MAdd,
            MC,
            MR,
            MS,
            MSub,
            Multiply,
            Negate,
            Nine,
            None,
            One,
            Percent,
            Seven,
            Six,
            Sqrt,
            Subtract,
            Three,
            Two,
            Zero
        }

        public enum Operation
        {
            Add,
            Subtract,
            Divide,
            Multiply,
            Percent,
            Sqrt,
            Fraction,
            None,
            Clear,
            Negate
        }

        #endregion //Enumerations

        #region Properties

        public ICommand CalculaterButtonClickCommand { get; private set; }

        #region CalculatorButtonType

        public static readonly DependencyProperty CalculatorButtonTypeProperty = DependencyProperty.RegisterAttached("CalculatorButtonType", typeof(CalculatorButtonType), typeof(Calculator), new UIPropertyMetadata(CalculatorButtonType.None, OnCalculatorButtonTypeChanged));
        public static CalculatorButtonType GetCalculatorButtonType(DependencyObject target)
        {
            return (CalculatorButtonType)target.GetValue(CalculatorButtonTypeProperty);
        }
        public static void SetCalculatorButtonType(DependencyObject target, CalculatorButtonType value)
        {
            target.SetValue(CalculatorButtonTypeProperty, value);
        }
        private static void OnCalculatorButtonTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            OnCalculatorButtonTypeChanged(o, (CalculatorButtonType)e.OldValue, (CalculatorButtonType)e.NewValue);
        }
        private static void OnCalculatorButtonTypeChanged(DependencyObject o, CalculatorButtonType oldValue, CalculatorButtonType newValue)
        {
            Button button = o as Button;
            button.CommandParameter = newValue;
            button.Content = CalculatorUtilities.GetCalculatorButtonContent(newValue);
        }

        #endregion //CalculatorButtonType

        #region DisplayText

        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText", typeof(string), typeof(Calculator), new UIPropertyMetadata("0", OnDisplayTextChanged));
        public string DisplayText
        {
            get { return (string)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        private static void OnDisplayTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Calculator calculator = o as Calculator;
            if (calculator != null)
                calculator.OnDisplayTextChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnDisplayTextChanged(string oldValue, string newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //DisplayText

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(decimal), typeof(Calculator), new UIPropertyMetadata(default(decimal), OnValueChanged));
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Calculator calculator = o as Calculator;
            if (calculator != null)
                calculator.OnValueChanged((decimal)e.OldValue, (decimal)e.NewValue);
        }

        protected virtual void OnValueChanged(decimal oldValue, decimal newValue)
        {
            DisplayText = newValue.ToString();
        }

        #endregion //Value

        #endregion //Properties

        #region Constructors

        static Calculator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Calculator), new FrameworkPropertyMetadata(typeof(Calculator)));
        }

        public Calculator()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;

            CommandBindings.Add(new CommandBinding(CalculatorCommands.CalculatorButtonClick, ExecuteCalculatorButtonClick));
            AddHandler(MouseDownEvent, new MouseButtonEventHandler(Calculator_OnMouseDown), true);
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _buttonPanel = (ContentControl)GetTemplateChild("PART_CalculatorButtonPanel");
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            var buttonType = CalculatorUtilities.GetCalculatorButtonTypeFromText(e.Text);
            if (buttonType != CalculatorButtonType.None)
            {
                SimulateCalculatorButtonClick(buttonType);
                ProcessCalculatorButton(buttonType);
            }
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        private void Calculator_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(_calculatorButton, _calculatorButton.IsMouseOver ? "MouseOver" : "Normal", true);
            _timer.Stop();
        }

        #endregion //Event Handlers

        #region Methods

        private void ProcessCalculatorButton(CalculatorButtonType buttonType)
        {
            if (CalculatorUtilities.IsDigit(buttonType))
                ProcessDigit(buttonType);
            else
                ProcessOperation(buttonType);
        }

        private void ProcessDigit(CalculatorButtonType buttonType)
        {
            if (_showNewNumber)
                DisplayText = CalculatorUtilities.GetCalculatorButtonContent(buttonType);
            else
                DisplayText += CalculatorUtilities.GetCalculatorButtonContent(buttonType);

            _showNewNumber = false;
        }

        private void ProcessOperation(CalculatorButtonType buttonType)
        {
            switch (buttonType)
            {
                case CalculatorButtonType.Add:
                    Calculate(Operation.Add);
                    break;
                case CalculatorButtonType.Subtract:
                    Calculate(Operation.Subtract);
                    break;
                case CalculatorButtonType.Multiply:
                    Calculate(Operation.Multiply);
                    break;
                case CalculatorButtonType.Divide:
                    Calculate(Operation.Divide);
                    break;
                case CalculatorButtonType.Percent:
                    Calculate(Operation.Percent);
                    break;
                case CalculatorButtonType.Sqrt:
                    Calculate(Operation.Sqrt, Operation.None);
                    break;
                case CalculatorButtonType.Fraction:
                    Calculate(Operation.Fraction, Operation.None);
                    break;
                case CalculatorButtonType.Negate:
                    Calculate(Operation.Negate, Operation.None);
                    break;
                case CalculatorButtonType.Equal:
                    Calculate(Operation.None);
                    break;
                case CalculatorButtonType.Clear:                    
                    Calculate(Operation.Clear, Operation.None);
                    DisplayText = Value.ToString();
                    break;
                case CalculatorButtonType.Cancel:
                    DisplayText = _previousValue.ToString();
                    _lastOperation = Operation.None;
                    _showNewNumber = true;
                    return;
                default:
                    break;
            }

            _previousValue = Decimal.Parse(DisplayText);
            _showNewNumber = true;
        }

        private void Calculate()
        {
            if (_lastOperation == Operation.None)
                return;

            Value = CalculateValue(_lastOperation);
        }

        private void Calculate(Operation newOperation)
        {
            if (!_showNewNumber)
                Calculate();

            _lastOperation = newOperation;
        }

        private void Calculate(Operation currentOperation, Operation newOperation)
        {
            _lastOperation = currentOperation;
            Calculate();
            _lastOperation = newOperation;
        }

        private decimal CalculateValue(Operation operation)
        {
            decimal newValue = decimal.Zero;
            decimal currentValue = Decimal.Parse(DisplayText);

            switch (operation)
            {
                case Operation.Add:
                    newValue = CalculatorUtilities.Add(_previousValue, currentValue);
                    break;
                case Operation.Subtract:
                    newValue = CalculatorUtilities.Subtract(_previousValue, currentValue);
                    break;
                case Operation.Multiply:
                    newValue = CalculatorUtilities.Multiply(_previousValue, currentValue);
                    break;
                case Operation.Divide:
                    newValue = CalculatorUtilities.Divide(_previousValue, currentValue);
                    break;
                case Operation.Percent:
                    newValue = CalculatorUtilities.Percent(_previousValue, currentValue);
                    break;
                case Operation.Sqrt:
                    newValue = CalculatorUtilities.SquareRoot(currentValue);
                    break;
                case Operation.Fraction:
                    newValue = CalculatorUtilities.Fraction(currentValue);
                    break;
                case Operation.Negate:
                    newValue = CalculatorUtilities.Negate(currentValue);
                    break;
                default:
                    newValue = decimal.Zero;
                    break;
            }

            return newValue;
        }

        private void SimulateCalculatorButtonClick(CalculatorButtonType buttonType)
        {
            _calculatorButton = CalculatorUtilities.FindButtonByCalculatorButtonType(_buttonPanel, buttonType);
            VisualStateManager.GoToState(_calculatorButton, "Pressed", true);
            _timer.Start();
        }

        #endregion //Methods

        #region Commands

        private void ExecuteCalculatorButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            var buttonType = (CalculatorButtonType)e.Parameter;
            ProcessCalculatorButton(buttonType);
        }

        #endregion //Commands
    }
}
