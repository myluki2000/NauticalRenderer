using System;
using System.Collections.Generic;
using System.Text;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using OsmSharp.Tags;

namespace NauticalRenderer.SlippyMap.UI
{
    sealed class OsmTagGrid : SingleItemContainer<Grid>
    {
        private TagsCollectionBase tagsCollection;

        public TagsCollectionBase TagsCollection
        {
            get => tagsCollection;
            set
            {
                tagsCollection = value; 
                RefreshGrid();
            }
        }

        public OsmTagGrid()
        {
            InternalChild = new Grid()
            {
                ShowGridLines = true,
                ColumnSpacing =  10,
                RowSpacing = 10,
            };
            InternalChild.ColumnsProportions.Add(Proportion.Auto);
            InternalChild.ColumnsProportions.Add(Proportion.Fill);
        }

        private void RefreshGrid()
        {
            int i = 0;
            foreach (Tag tag in tagsCollection)
            {
                InternalChild.RowsProportions.Add(Proportion.Auto);
                Label keyLabel = new Label()
                {
                    Text = tag.Key,
                    GridColumn = 0,
                    GridRow = i,
                };
                InternalChild.Widgets.Add(keyLabel);

                Label valueLabel = new Label()
                {
                    Text = tag.Value,
                    GridColumn = 1,
                    GridRow = i,
                    Wrap = true,
                };
                InternalChild.Widgets.Add(valueLabel);

                i++;
            }
        }
    }
}
