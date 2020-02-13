using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using Toolbox.Droid.List;

namespace Toolbox.Droid.Fragments
{
    public abstract class SmartExpandableListFragment<T, S> : BaseFragment,
        SmartExpandableListAdapter<T, S>.DataProvider
    {
        private SmartExpandableListAdapter<T, S> adapter;
        protected ExpandableListView ListView { get; private set; }
        protected SwipeRefreshLayout RefreshLayout { get; private set; }

        protected bool RefreshEnabled
        {
            get => RefreshLayout?.Enabled ?? true;
            set
            {
                if (RefreshLayout != null) RefreshLayout.Enabled = value;
            }
        }

        public abstract int SectionCount { get; }
        public abstract int RowsInSection(int sectionID);
        public abstract T ElementForSectionHeader(int sectionID);
        public abstract S ElementForIndex(int sectionId, int rowID);
        public abstract void RegisterCell(View cell, ViewHolder vh, bool isHeaderCell);
        public abstract void BindHeaderCell(ViewHolder vh, T item);
        public abstract void BindElementCell(ViewHolder vh, S item);
        public abstract int GetHeaderCellResource { get; }
        public abstract int GetElementCellResource { get; }

        public virtual bool IsItemSelectable(int sectionID, int roxID)
        {
            return true;
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            RefreshLayout = new SwipeRefreshLayout(Activity);
            RefreshLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent);

            ListView = new ExpandableListView(Activity);
            ListView.LayoutParameters =
                new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            RefreshLayout.AddView(ListView);

            adapter = new SmartExpandableListAdapter<T, S>(this);

            ListView.SetAdapter(adapter);

            RefreshLayout.Refresh += OnRefresh;

            return RefreshLayout;
        }

        protected void ReloadData()
        {
            try
            {
                Activity.RunOnUiThread(adapter.NotifyDataSetChanged);
            }
            catch
            {
                // ignored
            }
        }

        protected async Task<bool> ForceRefresh()
        {
            var completion = new TaskCompletionSource<bool>();
            RefreshDataSet(completion);
            try
            {
                var unused = await completion.Task;
                RefreshLayout.Refreshing = false;
                adapter.NotifyDataSetChanged();
                return true;
            }
            catch
            {
                RefreshLayout.Refreshing = false;
                return false;
            }
        }

        protected abstract void RefreshDataSet(TaskCompletionSource<bool> completion);

        private async void OnRefresh(object sender, EventArgs e)
        {
            await ForceRefresh();
        }
    }
}