using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Toofz.DBus.Introspection
{
    public sealed class DBusItemCollection : Collection<DBusItem>
    {
        public DBusItemCollection(Parent parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            this.parent = parent;
        }

        private Parent parent;

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        protected override void InsertItem(int index, DBusItem item)
        {
            item.Parent = parent;
            base.InsertItem(index, item);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        protected override void SetItem(int index, DBusItem item)
        {
            item.Parent = parent;
            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            Items[index].Parent = null;
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
                item.Parent = null;
            base.ClearItems();
        }
    }
}
