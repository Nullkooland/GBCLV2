using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBCLV2.Helpers
{
    static class KaomojiHelper
    {
        private static readonly string[] _kaomojis =
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

        public static string GetKaomoji()
        {
            int index = random.Next(_kaomojis.Length);
            return _kaomojis[index];
        }
    }
}
