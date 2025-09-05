namespace Bolt.Base
{
    public class EventfulForm : Form
    {
        protected EventfulForm() => InitializeEvents();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                TerminateEvents();

            base.Dispose(disposing);
        }

        protected virtual void InitializeEvents() { }
        protected virtual void TerminateEvents() { }
    }
}
