using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysProcessModel;
using System.ComponentModel;

namespace SysProcessViewModel
{
    public class ProStyleMatchingBO : INotifyPropertyChanged //ProStyleMatching
    {
        public int GroupID { get; set; }

        private IEnumerable<ProStyleMatching> _matchings;
        public IEnumerable<ProStyleMatching> Matchings
        {
            get
            {
                if (_matchings == null && GroupID != default(int))
                {
                    _matchings = VMGlobal.SysProcessQuery.LinqOP.Search<ProStyleMatching>(o => o.GroupID == GroupID).ToList();
                }
                return _matchings;
            }
            set
            {
                _matchings = value;
                OnPropertyChanged("Matchings");
                OnPropertyChanged("MatchPictures");
            }
        }

        private IEnumerable<ProSCPictureBO> _matchPictures;
        public IEnumerable<ProSCPictureBO> MatchPictures
        {
            get
            {
                if (_matchPictures == null && Matchings != null && Matchings.Count() > 0)
                {
                    var scids = Matchings.Select(o => o.StyleID + "-" + o.ColorID);
                    var pictures = VMGlobal.SysProcessQuery.LinqOP.Search<ProSCPicture>(o => scids.Contains(o.StyleID.ToString() + "-" + o.ColorID.ToString()));
                    //var data = from picture in pictures
                    //           from match in Matchings
                    //           where picture.StyleID == match.StyleID && picture.ColorID == match.ColorID
                    //           select new ProSCPictureBO(picture);
                    _matchPictures = pictures.Select(o => new ProSCPictureBO(o)).ToList();
                }
                return _matchPictures;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //public ProStyleMatchingBO()
        //{ }

        //public ProStyleMatchingBO(ProStyleMatching matching)
        //{
        //    this.CreatorID = matching.CreatorID;
        //    this.CreateTime = matching.CreateTime;
        //    this.GroupID = matching.GroupID;
        //    this.StyleID = matching.StyleID;
        //    this.ColorID = matching.ColorID;
        //}
    }
}
