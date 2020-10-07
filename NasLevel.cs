using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Tasks;
using MCGalaxy.Events.LevelEvents;
using BlockID = System.UInt16;
using Priority_Queue;

namespace NotAwesomeSurvival {

    public partial class NasLevel {
        static Scheduler TickScheduler;
        static TimeSpan tickDelay = TimeSpan.FromMilliseconds(100);
        static Random r = new Random();
        public class QueuedBlockUpdate {
            public int x, y, z;
            public NasBlock nb;
            public DateTime date;
        }
        
        [JsonIgnoreAttribute] public static Dictionary<string, NasLevel> all = new Dictionary<string, NasLevel>();
        [JsonIgnoreAttribute] public Level lvl;
        public ushort[,] heightmap;
        [JsonIgnoreAttribute] public SimplePriorityQueue<QueuedBlockUpdate, DateTime> tickQueue = new SimplePriorityQueue<QueuedBlockUpdate, DateTime>();
        [JsonIgnore] public SchedulerTask schedulerTask;
        
        public void BeginTickTask() {
            if (TickScheduler == null) TickScheduler = new Scheduler("NasLevelTickScheduler");
            schedulerTask = TickScheduler.QueueRepeat(TickLevelCallback, this, tickDelay);
        }
        public void EndTickTask() {
            TickScheduler.Cancel(schedulerTask);
        }
        static void TickLevelCallback(SchedulerTask task) {
            NasLevel nl = (NasLevel)task.State;
            nl.Tick();
        }
        public void Tick() {
            if (tickQueue.Count < 1) { return; }
            while (tickQueue.First.date < DateTime.UtcNow) {
                QueuedBlockUpdate qb = tickQueue.First;
                //lvl.Message("ticking for you gordon. in the test chamber");
                if (NasBlock.blocksIndexedByServerBlockID[lvl.GetBlock((ushort)qb.x, (ushort)qb.y, (ushort)qb.z)].selfID == qb.nb.selfID) {
                    qb.nb.disturbedAction(this, qb.x, qb.y, qb.z);
                }
                tickQueue.Dequeue();
                if (tickQueue.Count < 1) { break; }
            }
        }
        
        public void SetBlock(int x, int y, int z, BlockID serverBlockID) {
            if (
                x >= lvl.Width ||
                x < 0 ||
                y >= lvl.Height ||
                y < 0 ||
                z >= lvl.Length ||
                z < 0
               )
            { return; }
            lvl.Blockchange((ushort)x, (ushort)y, (ushort)z, serverBlockID);
            DisturbBlocks(x, y, z);
        }
        
        public void SimulateSetBlock(int x, int y, int z, BlockID serverBlockID) {
            if (
                x >= lvl.Width ||
                x < 0 ||
                y >= lvl.Height ||
                y < 0 ||
                z >= lvl.Length ||
                z < 0
               )
            { return; }
            lvl.SetBlock((ushort)x, (ushort)y, (ushort)z, serverBlockID);
            DisturbBlocks(x, y, z);
        }
        public void DisturbBlocks(int x, int y, int z) {
            DisturbBlock(x, y, z);
            
            DisturbBlock(x+1, y, z);
            DisturbBlock(x-1, y, z);
            
            DisturbBlock(x, y+1, z);
            DisturbBlock(x, y-1, z);
            
            DisturbBlock(x, y, z+1);
            DisturbBlock(x, y, z-1);
        }
        /// <summary>
        /// Call to make the nasBlock at this location queue its "whatHappensWhenDisturbed" function.
        /// </summary>
        private void DisturbBlock(int x, int y, int z) {
            if (
                x >= lvl.Width ||
                x < 0 ||
                y >= lvl.Height ||
                y < 0 ||
                z >= lvl.Length ||
                z < 0
               )
            { return; }
            
            NasBlock nb = NasBlock.blocksIndexedByServerBlockID[lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z)];
            if (nb.disturbedAction == null) { return; }
            QueuedBlockUpdate qb = new QueuedBlockUpdate();
            qb.x = x;
            qb.y = y;
            qb.z = z;
            float seconds = (float)(r.NextDouble() * (nb.disturbDelayMax - nb.disturbDelayMin) + nb.disturbDelayMin);
            qb.date = DateTime.UtcNow + TimeSpan.FromSeconds(seconds);
            qb.date = qb.date.Floor(tickDelay);
            qb.nb = nb;
            //lvl.Message("queueing thing "+qb.date.ToString("hh:mm:ss.fff tt"));
            tickQueue.Enqueue(qb, qb.date);
        }
        public BlockID GetBlock(int x, int y, int z) {
            return lvl.GetBlock((ushort)x, (ushort)y, (ushort)z);
        }

    }
    
    public static class DateExtensions {
        public static DateTime Round(this DateTime date, TimeSpan span) {
            long ticks = (date.Ticks + (span.Ticks / 2) + 1)/ span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }
        public static DateTime Floor(this DateTime date, TimeSpan span) {
            long ticks = (date.Ticks / span.Ticks);
            return new DateTime(ticks * span.Ticks);
        }
        public static DateTime Ceil(this DateTime date, TimeSpan span) {
            long ticks = (date.Ticks + span.Ticks - 1) / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }
    }

}
