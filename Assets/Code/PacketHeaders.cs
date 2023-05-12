using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PacketHeaders
{
    public enum WorldCommand : uint
    {
        SMSG_OFFER_ENTER = 0,
        CMSG_OFFER_ENTER_ANSWER = 1,

        SMSG_START_GAME = 2,

        CMSG_OBJ_INFO = 3,
        SMSG_OBJ_INFO = 4,
        CMSG_OBJ_CREATE = 5,
        SMSG_OBJ_CREATE = 6,
        CMSG_PLAYER_LOGIN = 7,
        SMSG_PLAYER_LOGIN = 8,
        SMSG_CREATE_PLAYERS = 9,

        CMSG_CREATE_BULLET = 10,
        SMSG_CREATE_BULLET = 11,

        CMSG_PLAYER_TAKE_DAMAGE = 12,
        SMSG_PLAYER_TAKE_DAMAGE = 13,

        CMSG_PLAYER_GIVE_DAMAGE = 14,
        SMSG_PLAYER_GIVE_DAMAGE = 15,

        CMSG_PLAYER_RESTORE_HEALTH = 16,
        SMSG_SET_PLAYER_HEALTH = 17,
        SMSG_PLAYER_DEATH = 18,
        SMSG_PLAYER_RESPAWN = 19,
        SMSG_REMOVE_PLAYER = 20,

        CMSG_PLAYER_WEAPON_INFO = 21,
        SMSG_PLAYER_WEAPON_INFO = 22,

        SMSG_CREATE_PICKUP = 23,
        SMSG_CREATE_PICKUP_COMPRESS = 24,
        SMSG_DESTROY_PICKUP = 25,
        CMSG_PLAYER_PICKUP_PICKUP = 26,

        CMSG_CREATE_BULLET_EFFECT = 27,
        SMSG_CREATE_BULLET_EFFECT = 28,

        SMSG_SET_PLAYER_IMPYLSE = 29,

        CMSG_SEND_MESSAGE = 30,
        SMSG_SEND_MESSAGE = 31,
        SMSG_CLEAR_PLAYER_CHAT = 32,

        SMSG_ADD_PLAYER_AMMO = 33,
        
        SMSG_SEND_KILL_MESSAGE = 34
    }
}
