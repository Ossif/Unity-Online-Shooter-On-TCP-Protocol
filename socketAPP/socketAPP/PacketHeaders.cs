using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socketAPP
{

        public enum WorldCommand : uint
        {
            MSG_NULL_ACTION = 0,
            SMSG_START_GAME = 1,

            CMSG_OBJ_INFO = 2,
            SMSG_OBJ_INFO = 3,
            CMSG_OBJ_CREATE = 4,
            SMSG_OBJ_CREATE = 5,
            CMSG_PLAYER_LOGIN = 6,
            SMSG_PLAYER_LOGIN = 7,
            SMSG_CREATE_PLAYERS = 8,

            CMSG_CREATE_BULLET = 9,
            SMSG_CREATE_BULLET = 10
        }
}
