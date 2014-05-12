namespace Toofz.DBus.Introspection
{
    public abstract class Parent : DBusItem
    {
        protected Parent() : this(null) { }
        protected Parent(string name) : base(name) { }

        private DBusItemCollection _InnerCollection;
        protected DBusItemCollection InnerCollection { get { return _InnerCollection ?? (_InnerCollection = new DBusItemCollection(this)); } }

        public virtual void Add(DBusItem child)
        {
            InnerCollection.Add(child);
        }
    }
}
