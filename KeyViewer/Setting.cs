using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityModManagerNet;

namespace ShowHitMargin_Eng
{
    public class Setting : UnityModManager.ModSettings
    {
        public bool onShowHitMargin = true;
        public bool useShadow = true;
        public bool useBold = false;
        //public bool allowCollectInfo = false;
        
        public float x = 0.96f, y=0.98f;
        public int size = 35;
        public int align = 2;
        public int showDecimal = 0;
        public bool zero = true;

        public string text4 = "HitMargin - {value}";


        public override void Save(UnityModManager.ModEntry modEntry) {
            var filepath = GetPath(modEntry);
            try {
                using (var writer = new StreamWriter(filepath)) {
                    var serializer = new XmlSerializer(GetType());
                    serializer.Serialize(writer, this);
                }
            } catch (Exception e) {
                modEntry.Logger.Error($"Can't save {filepath}.");
                modEntry.Logger.LogException(e);
            }
        }
        
        public override string GetPath(UnityModManager.ModEntry modEntry) {
            return Path.Combine(modEntry.Path, GetType().Name + ".xml");
        }

    }
}
