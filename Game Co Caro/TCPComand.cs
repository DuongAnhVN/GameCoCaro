using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Co_Caro
{
    public class TCPComand
    {
        public static string SEND_POINT = "SENDPOINT";
        public static string NEW_GAME = "NEWGAME";
        public static string CANCEL_GAME = "CANCELGAME";
        public static string CONTINUE_GAME = "CONTINUEGAME";
        public static string XIN_THUA = "XINTHUA";
        public static string UNDO = "";

        public static string Command(string para)
        {
            return $"/PHONGGAME {para}";
        }

        public static string Chat(string para)
        {
            return $"/CHAT {para}";
        }
    }
}
