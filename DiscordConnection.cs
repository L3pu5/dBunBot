using System;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace DiscordConnection {

    public static class API{
        public static string BaseURL = "https://discord.com/api";
    }

    //This handles the Gateway for the Discord Messages with the bot.
    class GateWay{
        //Opening Codes
        enum Opcode {Dispatch, Heartbeat, Identify, PresenceUpdate, VoiceStateUpdate, Resume, Reconnect, RequestGuildMembers, InvalidSession, Hello, HeartbeatAck};
        //Close Event Codes
        enum CloseCode{UnkownError=4000, UnkownOpcode=4001, DecodeError=4002, NotAuthenticated=4003, AuthenticationFailed=4004, AlreadyAuthenticated=4005, 
        InvalidSeq=4007, RateLimited=4008, SessionTimedOut=4009, InvalidShard=4010, ShardingRequired=4011, InvalidAPIVersion=4012, InvalidIntent =4013,
        DisallowedIntent=4014};
    }

    class Bot{
        //Size of read Buffer in Bytes
        public int BufferSize = 512;



    }
}