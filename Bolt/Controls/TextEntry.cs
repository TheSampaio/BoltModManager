using System.ComponentModel;

namespace Bolt.Controls
{
    public partial class TextEntry : UserControl
    {
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        [Category("Appearance")]
        [Description("The text associated with the control.")]
        public override string Text
        {
            get => LblTextEntry?.Text ?? string.Empty;
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
            set
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
            {
                if (LblTextEntry != null)
                    LblTextEntry.Text = value;

                // also call base.Text so the designer and accessibility tools 
                // still have the correct value
                base.Text = value;
            }
        }

        public TextEntry()
        {
            InitializeComponent();
            base.Text = LblTextEntry.Text; // sync initial value
        }
    }
}
