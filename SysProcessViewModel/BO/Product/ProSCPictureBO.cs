using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysProcessModel;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace SysProcessViewModel
{
    public class ProSCPictureBO : ProSCPicture
    {
        //private bool _isThumbnail = true;
        //public bool IsThumbnail { get { return _isThumbnail; } set { _isThumbnail = value; } }

        private ImageSource _picture = null;
        public ImageSource Picture
        {
            get
            {
                if (_picture == null)
                {
                    _picture = ProductHelper.GetProductImage(this, false);
                }
                return _picture;
            }
            set
            {
                _picture = value;
            }
        }

        private ImageSource _thumbnailPicture = null;
        public ImageSource ThumbnailPicture
        {
            get
            {
                if (_thumbnailPicture == null)
                {
                    _thumbnailPicture = ProductHelper.GetProductImage(this);
                }
                return _thumbnailPicture;
            }
            set
            {
                _thumbnailPicture = value;
            }
        }

        private ObservableCollection<ProStyleMatchingBO> _matchings;
        public ObservableCollection<ProStyleMatchingBO> Matchings
        {
            get
            {
                if (_matchings == null)
                {
                    var gids = VMGlobal.SysProcessQuery.LinqOP.Search<ProStyleMatching, int>(o => o.GroupID, o => o.StyleID == this.StyleID && o.ColorID == this.ColorID).ToList();
                    _matchings = new ObservableCollection<ProStyleMatchingBO>(gids.Select(o => new ProStyleMatchingBO { GroupID = o }).ToList());
                }
                return _matchings;
            }
            //set { _matchings = value;}
        }

        //private IEnumerable<ProSCPictureBO> _matchPictures;
        //public IEnumerable<ProSCPictureBO> MatchPictures
        //{
        //    get
        //    {
        //        if (_matchPictures == null)
        //        {
        //            var matches = VMGlobal.SysProcessQuery.LinqOP.GetDataContext<ProStyleMatching>();
        //            var styleMatches = VMGlobal.SysProcessQuery.LinqOP.Search<ProStyleMatching>(o => o.StyleID == this.StyleID && o.ColorID == this.ColorID);
        //            var pictures = VMGlobal.SysProcessQuery.LinqOP.GetDataContext<ProSCPicture>();
        //            var data = from picture in pictures
        //                       from match in matches
        //                       where picture.StyleID == match.StyleID && picture.ColorID == match.ColorID
        //                       from sm in styleMatches
        //                       where match.GroupID == match.GroupID
        //                       select new ProSCPictureBO(picture);
        //            _matchPictures = data.ToList();
        //        }
        //        return _matchPictures;
        //    }
        //}

        public ProSCPictureBO()
        { }

        public ProSCPictureBO(ProSCPicture scpicture)
        {
            //this.BYQID = scpicture.BYQID;
            this.PictureName = scpicture.PictureName;
            //this.SCCode = scpicture.SCCode;
            this.StyleID = scpicture.StyleID;
            this.ColorID = scpicture.ColorID;
            this.UploadTime = scpicture.UploadTime;
        }
    }
}
