using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;
using SysProcessModel;
using System.ComponentModel;
using System.Windows.Input;
using Kernel;
using System.Transactions;
using System.Collections.ObjectModel;

namespace SysProcessViewModel
{
    public class WinStyleSelectForMatchingVM : CommonViewModel<ProSCPictureForMatchingBO>
    {
        public static RoutedCommand PictureRoutedCommand = new RoutedCommand();

        private ProStyleMatchingBO _matching = null;

        private StylePictureAlbum _album = null;

        public IEnumerable<ProSCPictureForMatchingBO> SelectedPictures
        {
            get
            {
                if (this.Entities == null)
                    return null;
                return this.Entities.Where(o => o.IsSelected);
            }
        }

        public WinStyleSelectForMatchingVM(StylePictureAlbum album)
        {
            _album = album;
            Entities = this.SearchData();
        }

        public WinStyleSelectForMatchingVM(StylePictureAlbum album, ProStyleMatchingBO matching)
            : this(album)
        {
            _matching = matching;
            if (_matching != null && Entities != null)
            {
                //var matchings = VMGlobal.SysProcessQuery.LinqOP.Search<ProStyleMatching>(o => o.GroupID == groupID).ToList();
                var matchings = matching.Matchings.ToList();
                matchings.ForEach(o =>
                {
                    var styles = Entities.Where(e => e.StyleID == o.StyleID && e.ColorID == o.ColorID);
                    foreach (var s in styles)
                        s.IsSelected = true;
                });
            }
        }

        protected override IEnumerable<ProSCPictureForMatchingBO> SearchData()
        {
            if (_album == null)
                return null;
            //var lp = VMGlobal.SysProcessQuery.LinqOP;
            //var styles = lp.Search<ProStyle>(o => o.BYQID == _album.SelectedStyle.BYQID);
            //var scpictures = lp.GetDataContext<ProSCPicture>();
            //var data = from style in styles
            //           from scpicture in scpictures
            //           where style.ID == scpicture.StyleID
            //           select scpicture;
            //var list = data.Select(o => new ProSCPictureForMatchingBO(o)).ToList();
            //list.RemoveAll(o => o.StyleID == _album.ID);
            var list = _album.Styles.Where(o => o.ID != _album.SelectedStyle.ID).SelectMany(o => o.Pictures).Select(o => new ProSCPictureForMatchingBO(o)).ToList();
            return list.OrderBy(o => o.StyleID);
        }

        public void SelectPicturePerStyle(ProSCPictureForMatchingBO pic)
        {
            if (this.Entities != null || this.Entities.Count() > 0)
            {
                if (pic.IsSelected)
                {
                    var data = this.Entities.Where(o => o.StyleID == pic.StyleID && o.ColorID != pic.ColorID && o.IsSelected);
                    foreach (var d in data)
                    {
                        d.IsSelected = false;//telerik控件有bug，IsSelected貌似不能正常反馈到RadListItem的IsSelected属性（设为false之后，UI层需要点击两次才能重置为true）
                        //直接在UI层设置false也会出现同样问题
                        //估计SelectionMode设为Multiple会出现此类问题
                        //只好注册容器的MouseLeftButtonUp的事件进行处理，请看UI的Grid_MouseLeftButtonUp方法
                        //最终决定还是将RadListBox替换成原生的ListBox
                    }
                }
            }
            OnPropertyChanged("SelectedPictures");
        }

        public OPResult Save()
        {
            if (SelectedPictures != null && SelectedPictures.Count() > 0)
            {
                var lp = VMGlobal.SysProcessQuery.LinqOP;
                int gid = _matching == null ? 0 : _matching.GroupID;
                if (gid == 0)
                    gid = lp.GetDataContext<ProStyleMatching>().Max(o => o.GroupID) + 1;
                var list = SelectedPictures.Select(o => new ProStyleMatching
                {
                    ColorID = o.ColorID,
                    CreateTime = DateTime.Now,
                    CreatorID = VMGlobal.CurrentUser.ID,
                    StyleID = o.StyleID,
                    GroupID = gid
                }).ToList();
                list.Add(new ProStyleMatching
                {
                    ColorID = _album.SelectedStyle.SelectedPicture.ColorID,
                    CreateTime = DateTime.Now,
                    CreatorID = VMGlobal.CurrentUser.ID,
                    StyleID = _album.SelectedStyle.ID,
                    GroupID = gid
                });
                var matching = _album.SelectedStyle.SelectedPicture.Matchings.FirstOrDefault(o => o.GroupID != gid && this.ContainOther(o.Matchings, list));
                if (matching != null)
                    return new OPResult { IsSucceed = false, Message = "已有其它搭配组包含重复的搭配款色,请避免款色的重复搭配." };
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        if (_matching != null)
                        {
                            lp.Delete<ProStyleMatching>(o => o.GroupID == _matching.GroupID);
                        }
                        lp.Add<ProStyleMatching>(list);
                        scope.Complete();
                    }
                    catch (Exception e)
                    {
                        return new OPResult { IsSucceed = false, Message = "保存失败,原因:" + e.Message };
                    }
                }
                if (_matching != null)
                {
                    ProStylePictureBookVM.DeleteMatchingForAlbum(_album, _matching);
                }
                ProStylePictureBookVM.AddMatchingForAlbum(_album, new ProStyleMatchingBO { GroupID = gid, Matchings = list });
                return new OPResult { IsSucceed = true, Message = "保存成功!" };
            }
            else
                return new OPResult { IsSucceed = false, Message = "没有可保存的数据." };
        }

        /// <summary>
        /// 判断俩搭配组是否重叠，即1包含2或2包含1
        /// <remarks>注意交叉情况不算,返回false</remarks>
        /// </summary>
        private bool ContainOther(IEnumerable<ProStyleMatching> matching1, IEnumerable<ProStyleMatching> matching2)
        {
            if (matching1.Count() < matching2.Count())
            {
                var temp = matching1;
                matching1 = matching2;
                matching2 = temp;
            }
            //m2包含有m1不包含的数据，则返回false
            bool flag = matching2.Any(o2 =>
            {
                return !matching1.Any(o1 => o1.StyleID == o2.StyleID && o1.ColorID == o2.ColorID);
            });
            return !flag;
        }
    }

    public class ProSCPictureForMatchingBO : ProSCPictureBO, INotifyPropertyChanged
    {
        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                    //if (value)
                    WinStyleSelectForMatchingVM.PictureRoutedCommand.Execute(this, null);
                    //OnPropertyChanged("SelectedPictures");//放在这里似乎有问题，不能正常反馈到界面？
                }
            }
        }

        public ProSCPictureForMatchingBO() { }

        public ProSCPictureForMatchingBO(ProSCPicture scpicture) : base(scpicture) { }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
