using System.ComponentModel;
using System.Windows.Forms;

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
#pragma warning restore CS8765
            {
                if (LblTextEntry is not null)
                    LblTextEntry.Text = value;

                // Also call base.Text so the designer and accessibility tools
                // Still have the correct value
                base.Text = value;
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        [Category("Behavior")]
        [Description("Indicates whether the text box is read-only.")]
        public bool ReadOnly
        {
            get => TxtTextEntry?.ReadOnly ?? false;
            set
            {
                if (TxtTextEntry is not null)
                    TxtTextEntry.ReadOnly = value;
            }
        }

        public TextEntry()
        {
            InitializeComponent();

            // Sync initial value
            base.Text = LblTextEntry.Text;
        }
    }
}
