using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CentralizeModel;
using Telerik.Windows.Controls;
using System.Windows.Documents;
//using Telerik.Windows.Documents.FormatProviders;
//using Telerik.Windows.Documents.Model;
//using Telerik.Windows.Documents.FormatProviders.Xaml;
using System.ComponentModel;

namespace SysProcessViewModel
{
    public class SoftVersionTrackBO : SoftVersionTrack,INotifyPropertyChanged
    {
        #region 属性

        public override string VersionCode
        {
            get
            {
                return base.VersionCode;
            }
            set
            {
                base.VersionCode = value;
                OnPropertyChanged("VersionCode");
            }
        }

        public override bool IsCoerciveUpdate
        {
            get
            {
                return base.IsCoerciveUpdate;
            }
            set
            {
                base.IsCoerciveUpdate = value;
                OnPropertyChanged("IsCoerciveUpdateStr");
            }
        }

        public string IsCoerciveUpdateStr { get { return IsCoerciveUpdate ? "是" : "否"; } }

        //改写为这种模式期望能按需加载，但在2012.2.607版中RadBook控件仍然一次性将所有数据加载完毕
        //public override byte[] Description
        //{
        //    get
        //    {
        //        if (base.Description == null)
        //        {
        //            var track = VMGlobal.PlatformCentralizeQuery.LinqOP.Search<SoftVersionTrack>(o => o.ID == this.ID).FirstOrDefault();
        //            if (track != null)
        //                base.Description = track.Description;
        //            else
        //                base.Description = new byte[] { };
        //        }
        //        return base.Description;
        //    }
        //}

        //private FlowDocument _flowDescription;
        //public FlowDocument FlowDescription
        //{
        //    get
        //    {
        //        if (_flowDescription == null)
        //        {
        //            MsRichTextBoxXamlFormatProvider formator = new MsRichTextBoxXamlFormatProvider();
        //            _flowDescription = formator.ExportToFlowDocument(((IDocumentFormatProvider)formator).Import(this.Description));
        //        }
        //        return _flowDescription;
        //    }
        //}

        #endregion

        public SoftVersionTrackBO()
        { }

        public SoftVersionTrackBO(SoftVersionTrack softversion)
        {
            this.ID = softversion.ID;
            this.SoftID = softversion.SoftID;
            this.IsCoerciveUpdate = softversion.IsCoerciveUpdate;
            this.UpdatedFileList = softversion.UpdatedFileList;
            this.VersionCode = softversion.VersionCode;
            //this.Description = softversion.Description;
            this.CreateTime = softversion.CreateTime;
        }

        #region 类型转换操作符重载

        //public static implicit operator SoftVersionTrackBO(SoftVersionTrack softversion)
        //{
        //    return new SoftVersionTrackBO(softversion);
        //}

        //public static implicit operator SoftVersionTrack(SoftVersionTrackBO softversion)
        //{
        //    return new SoftVersionTrack
        //    {
        //        ID = softversion.ID,
        //        IsCoerciveUpdate = softversion.IsCoerciveUpdate,
        //        UpdatedFileList = softversion.UpdatedFileList,
        //        VersionCode = softversion.VersionCode,
        //        Description = softversion.Description,
        //        CreateTime = softversion.CreateTime,
        //        SoftID = softversion.SoftID
        //    };
        //}

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
