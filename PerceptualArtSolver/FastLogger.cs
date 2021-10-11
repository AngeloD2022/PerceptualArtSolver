using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace PerceptualArtSolver
{
    public enum FLMessageType
    {
        ERROR,
        WARNING,
        INFO,
        CONFIRM
    }
    
    public struct FLMessage
    {
        DateTime Time;
        string MethodName;
        string Content;
        FLMessageType MessageType;

        public FLMessage(FLMessageType type, string methodName, string content)
        {
            this.Time = DateTime.Now;
            this.Content = content;
            this.MessageType = type;
            this.MethodName = methodName;
        }
        
        public override string ToString()
        {
            string icon = (MessageType == FLMessageType.INFO ? "ℹ️" : MessageType == FLMessageType.ERROR ? "❌" : MessageType == FLMessageType.WARNING ? "⚠️": MessageType == FLMessageType.CONFIRM ? "✅" : "ℹ️");
            return $"@ [{Time.Hour}:{Time.Minute}:{Time.Second}] [{icon} @ {MethodName}()]: {Content}";
        }
    }
    
    public class FastLogger
    {
        public readonly string OperationName;
        private List<FLMessage> messages;
        
        public FastLogger(string operationName)
        {
            OperationName = operationName;
            messages = new List<FLMessage>();
            
            messages.Add(new FLMessage(FLMessageType.INFO, "FastLogger", "Operation logging started."));
        }

        public void Log(FLMessageType type, string message, int stackIndex = 1)
        {
            // Automatically get the method name of the log call.
            StackTrace st = new StackTrace();
            string methodName = st.GetFrames()[stackIndex].GetMethod().Name;
            
            messages.Add(new FLMessage(type, methodName, message));
        }

        public void Error(string message)
        {
            Log(FLMessageType.ERROR, message, stackIndex:2);
        }
        
        public void Info(string message)
        {
            Log(FLMessageType.INFO, message, stackIndex:2);
        }
        
        public void Confirm(string message)
        {
            Log(FLMessageType.CONFIRM, message, stackIndex:2);
        }
        
        public void Warning(string message)
        {
            Log(FLMessageType.WARNING, message, stackIndex:2);
        }
        
        public override string ToString()
        {
            string buf = $"------------------ LOG: {OperationName} ------------------";
            foreach (var msg in messages)
            {
                buf += msg.ToString() + '\n';
            }
            
            return buf;
        }

        public void WriteToLogs()
        {
            File.WriteAllText($"/Users/angelodeluca/Desktop/ModelDebug/logs/{OperationName}.log", ToString());
        }
    }
}