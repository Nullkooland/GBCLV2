using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBCLV2.Helpers
{
    static class TextFacesHelper
    {
        private static readonly string[] _textFaces =
        {
            " (⁄ ⁄•⁄ω⁄•⁄ ⁄)",
            "(⇀‸↼‶)",
            "(๑˘•◡•˘๑)",
            "( Ծ ‸ Ծ )",
            "_( '-' _)⌒)_",
            "(●—●)",
            "~( ´•︵•` )~",
            "( *・ω・)✄╰ひ╯",
            "(╯>д<)╯┻━┻",
            "_(-ω-`_)⌒)_",
            "ᕦ(･ㅂ･)ᕤ",
            "(◞‸◟ )",
            "(ㅎ‸ㅎ)",
            "(= ᵒᴥᵒ =)",
            "_(¦3」∠)_ ",
            "(๑乛◡乛๑)",
            "( ,,ÒωÓ,, )",
            "ε=ε=(ノ≧∇≦)ノ",
            "(･∀･)",
            "Σ( ￣□￣||)",
            "(。-`ω´-)",
            "(๑•́ ₃ •̀๑)",
            "(´• ᗜ •`)",
            "(๑╹∀╹๑)",
            "(´• ᵕ •`)*✲",
            "┑(￣Д ￣)┍",
            "(≖＿≖)✧ ",
            "(｡•ˇ‸ˇ•｡)",
            "\\(•ㅂ•)/",
            "(´･ᆺ･`)",
            "ԅ(¯﹃¯ԅ)",
            "୧(๑•∀•๑)૭",
            "ʕ•ﻌ•ʔ",
            "ヾ(*´∀ ˋ*)ﾉ ",
            "ヽ(●´∀`●)ﾉ ",
            "d(`･∀･)b ",
        };

        private static Random random = new Random();

        public static string GetTextFace()
        {
            int index = random.Next(_textFaces.Length);
            return _textFaces[index];
        }
    }
}
