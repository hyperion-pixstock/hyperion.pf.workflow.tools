using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xmi2Stm.Model
{
    public abstract class Event
    {
        /// <summary>
        /// イベント名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// メソッド名(ある場合)
        /// </summary>
        public string MethodName { get; set; }
    }

    public class InternalEvent : Event
    {
        public PrefixType Prefix { get; set; } = PrefixType.NONE;
    }

    public class TransitionEvent : Event
    {
        public NormalState Destination { get; set; }
    }

    public enum PrefixType
    {
        NONE = 0,
        RIBBONEVENT
    }
}
