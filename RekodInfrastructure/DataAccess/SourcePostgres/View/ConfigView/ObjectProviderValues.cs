using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace Rekod.DataAccess.SourcePostgres.View.ConfigView
{
    public struct StyleHatch
    {
        public StyleHatch(int style, int hatch)
        {
            Style = style;
            Hatch = hatch; 
        }
        public Int32 Style;
        public Int32 Hatch; 
    }
    public class EnumWrapper
    {
        private String _description; 
        public System.Enum Value
        {
            get;
            set; 
        }
        public String Description
        {
            get
            {
                if (!String.IsNullOrEmpty(_description))
                {
                    return _description; 
                }
                Type enumType = Value.GetType();
                object[] attrs = enumType.GetCustomAttributes(typeof(Rekod.Services.TypeResourceAttribute), false);
                if (attrs.Count() == 1)
                {
                    Rekod.Services.TypeResourceAttribute localeDescription = attrs[0] as Rekod.Services.TypeResourceAttribute;
                    String resKey = localeDescription.ResourceKey + "." + Value;
                    String resValue = Properties.Resources.ResourceManager.GetString(resKey, Properties.Resources.Culture);
                    if (String.IsNullOrEmpty(resValue))
                    {
                        //throw new NotImplementedException(String.Format("Не найден ресурс \"{0}\"", resKey));
                        System.Windows.MessageBox.Show(String.Format("Не найден ресурс \"{0}\"", resKey));
                    }
                    _description = resValue;
                }
                else
                {
                    throw new NotImplementedException(String.Format("У типа перечисления \"{0}\" не найден атрибут \"{1}\"", enumType, typeof(Rekod.Services.TypeResourceAttribute))); 
                }
                return _description; 
            }
        }
        public override string ToString()
        {
            return Description; 
        }
    }
    public class ObjectProviderValues
    {
        public static List<BitmapSource> Pens
        {
            get;
            private set; 
        }
        public static Dictionary<StyleHatch, BitmapSource> StyleHatchToBitmapSource
        {
            get;
            private set; 
        }
        public static Dictionary<BitmapSource, StyleHatch> BitmapSourceToStyleHatch
        {
            get;
            private set;
        }
        public static List<char> GetSymbols()
        {
            List<char> charList = new List<char>();
            for (int i = 32; i < 256; i++)
            {
                charList.Add((char)i);
            }
            return charList;
        }
        public static List<BitmapSource> GetPensImages()
        {
            if (Pens == null)
            {
                Pens = new List<BitmapSource>(); 

                List<Bitmap> bmpList = new List<Bitmap>();
                bmpList.Add(global::Rekod.lineRes.LINE01);
                bmpList.Add(global::Rekod.lineRes.LINE02);
                bmpList.Add(global::Rekod.lineRes.LINE03);
                bmpList.Add(global::Rekod.lineRes.LINE04);
                bmpList.Add(global::Rekod.lineRes.LINE05);
                bmpList.Add(global::Rekod.lineRes.LINE06);
                bmpList.Add(global::Rekod.lineRes.LINE07);
                bmpList.Add(global::Rekod.lineRes.LINE08);
                bmpList.Add(global::Rekod.lineRes.LINE09);
                bmpList.Add(global::Rekod.lineRes.LINE10);
                bmpList.Add(global::Rekod.lineRes.LINE11);
                bmpList.Add(global::Rekod.lineRes.LINE12);
                bmpList.Add(global::Rekod.lineRes.LINE13);
                bmpList.Add(global::Rekod.lineRes.LINE14);
                bmpList.Add(global::Rekod.lineRes.LINE15);
                bmpList.Add(global::Rekod.lineRes.LINE16);
                bmpList.Add(global::Rekod.lineRes.LINE17);
                bmpList.Add(global::Rekod.lineRes.LINE18);
                bmpList.Add(global::Rekod.lineRes.LINE19);
                bmpList.Add(global::Rekod.lineRes.LINE20);
                bmpList.Add(global::Rekod.lineRes.LINE21);
                bmpList.Add(global::Rekod.lineRes.LINE22);
                bmpList.Add(global::Rekod.lineRes.LINE23);
                bmpList.Add(global::Rekod.lineRes.LINE24);
                bmpList.Add(global::Rekod.lineRes.LINE25);
                foreach (Bitmap bmp in bmpList)
                {
                    BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        bmp.GetHbitmap(), IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    Pens.Add(bmpSource);
                }
            }
            return Pens;
        }
        public static IEnumerable<BitmapSource> GetBrushesImages()
        {
            if (StyleHatchToBitmapSource == null)
            {
                StyleHatchToBitmapSource = new Dictionary<StyleHatch, BitmapSource>();
                StyleHatchToBitmapSource.Add(new StyleHatch(1, 0), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_1));
                StyleHatchToBitmapSource.Add(new StyleHatch(0, 0), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_2));
                StyleHatchToBitmapSource.Add(new StyleHatch(2, 0), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_2_0));
                StyleHatchToBitmapSource.Add(new StyleHatch(2, 1), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_2_1));
                StyleHatchToBitmapSource.Add(new StyleHatch(2, 2), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_2_2));
                StyleHatchToBitmapSource.Add(new StyleHatch(2, 3), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_2_3));
                StyleHatchToBitmapSource.Add(new StyleHatch(2, 4), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_2_4));
                StyleHatchToBitmapSource.Add(new StyleHatch(2, 5), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_2_5));

                //StyleHatchToBitmapSource.Add(new StyleHatch(5, 3), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_3));
                //StyleHatchToBitmapSource.Add(new StyleHatch(5, 4), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_4));
                //StyleHatchToBitmapSource.Add(new StyleHatch(5, 5), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_5));
                //StyleHatchToBitmapSource.Add(new StyleHatch(5, 6), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_6));
                //StyleHatchToBitmapSource.Add(new StyleHatch(5, 7), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_7));
                //StyleHatchToBitmapSource.Add(new StyleHatch(5, 8), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_8));

                StyleHatchToBitmapSource.Add(new StyleHatch(5, 12), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_12));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 13), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_13));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 14), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_14));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 15), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_15));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 16), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_16));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 17), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_17));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 18), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_18));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 19), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_19));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 20), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_20));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 21), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_21));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 22), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_22));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 23), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_23));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 24), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_24));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 25), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_25));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 26), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_26));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 27), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_27));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 28), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_28));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 29), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_29));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 30), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_30));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 31), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_31));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 32), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_32));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 33), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_33));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 34), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_34));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 35), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_35));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 36), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_36));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 37), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_37));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 38), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_38));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 39), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_39));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 40), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_40));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 41), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_41));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 42), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_42));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 43), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_43));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 44), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_44));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 45), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_45));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 46), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_46));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 47), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_47));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 48), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_48));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 49), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_49));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 50), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_50));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 51), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_51));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 52), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_52));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 53), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_53));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 54), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_54));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 55), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_55));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 56), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_56));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 57), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_57));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 58), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_58));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 59), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_59));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 60), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_60));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 61), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_61));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 62), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_62));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 63), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_63));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 64), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_64));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 65), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_65));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 66), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_66));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 67), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_67));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 68), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_68));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 69), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_69));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 70), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_70));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 71), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_71));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 72), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_72));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 73), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_73));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 74), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_74));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 75), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_75));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 76), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_76));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 77), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_77));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 78), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_78));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 79), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_79));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 80), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_80));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 81), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_81));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 82), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_82));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 83), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_83));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 84), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_84));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 85), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_85));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 86), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_86));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 87), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_87));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 88), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_88));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 89), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_89));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 90), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_90));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 91), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_91));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 92), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_92));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 93), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_93));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 94), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_94));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 95), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_95));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 96), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_96));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 97), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_97));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 98), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_98));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 99), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_99));
                #region 100
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 100), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_100));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 101), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_101));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 102), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_102));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 103), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_103));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 104), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_104));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 105), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_105));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 106), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_106));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 107), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_107));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 108), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_108));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 109), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_109));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 110), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_110));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 111), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_111));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 112), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_112));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 113), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_113));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 114), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_114));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 115), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_115));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 116), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_116));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 117), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_117));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 118), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_118));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 119), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_119)); 
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 120), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_120));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 121), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_121));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 122), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_122));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 123), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_123));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 124), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_124));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 125), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_125));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 126), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_126));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 127), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_127));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 128), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_128));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 129), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_129));
                
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 130), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_130));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 131), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_131));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 132), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_132));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 133), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_133));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 134), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_134));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 135), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_135));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 136), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_136));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 137), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_137));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 138), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_138));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 139), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_139));
                
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 140), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_140));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 141), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_141));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 142), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_142));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 143), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_143));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 144), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_144));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 145), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_145));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 146), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_146));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 147), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_147));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 148), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_148));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 149), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_149));
                
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 150), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_150));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 151), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_151));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 152), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_152));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 153), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_153));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 154), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_154));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 155), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_155));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 156), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_156));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 157), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_157));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 158), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_158));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 159), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_159));
                
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 160), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_160));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 161), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_161));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 162), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_162));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 163), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_163));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 164), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_164));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 165), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_165));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 166), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_166));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 167), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_167));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 168), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_168));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 169), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_169));
                
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 170), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_170));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 171), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_171));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 172), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_172));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 173), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_173));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 174), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_174));
                StyleHatchToBitmapSource.Add(new StyleHatch(5, 175), getBmpSrcByBmp(global::Rekod.ImageBrushRes.poly_5_175));
                #endregion

            }
            if (BitmapSourceToStyleHatch == null)
            {
                BitmapSourceToStyleHatch = new Dictionary<BitmapSource, StyleHatch>();
                var enumerator = StyleHatchToBitmapSource.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var pair = enumerator.Current;
                    BitmapSourceToStyleHatch.Add(pair.Value, pair.Key); 
                }
            }
            return StyleHatchToBitmapSource.Values; 
        }
        public static Dictionary<Type, List<EnumWrapper>> EnumValues { get; private set; }
        private static BitmapSource getBmpSrcByBmp(Bitmap bmp)
        {
            Rekod.DataAccess.AbstractSource.Model.EGeomType geomType = AbstractSource.Model.EGeomType.None;
            System.Enum en = geomType; 

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bmp.GetHbitmap(), IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
        }
        /// <summary>
        /// Получить список значений перечисления, которые будут использоваться в интерфейсе пользователя
        /// </summary>
        /// <param name="enumType">Тип перечисления</param>
        /// <param name="values">Значения из перечисления</param>
        /// <param name="include">Включить или исключить значения из values</param>       
        /// <returns></returns>
        public static List<EnumWrapper> GetEnumValues(Type enumType, EnumCollection values = null, Boolean include = true)
        {
            if (EnumValues == null)
            {
                EnumValues = new Dictionary<Type, List<EnumWrapper>>();
            }
            if (!EnumValues.ContainsKey(enumType))
            {
                object[] attrs = enumType.GetCustomAttributes(typeof(Rekod.Services.TypeResourceAttribute), false);
                if (attrs.Count() == 1)
                {
                    List<EnumWrapper> enumWrappers = new List<EnumWrapper>();
                    foreach (System.Enum enumValue in Enum.GetValues(enumType))
                    {
                        enumWrappers.Add(new EnumWrapper() { Value = enumValue });
                    }
                    EnumValues.Add(enumType, enumWrappers); 
                }
            }
            List<EnumWrapper> result = null;
            if (EnumValues.ContainsKey(enumType))
            {
                if (values == null)
                {
                    result = EnumValues[enumType];
                }
                else 
                {
                    result =
                       (from EnumWrapper enumWrapper
                           in EnumValues[enumType]
                        where values.Contains(enumWrapper.Value) == include
                        select enumWrapper).ToList();
                }
            }
            return result;
        }
    }
    public class EnumCollection : ObservableCollection<Enum> { }
}