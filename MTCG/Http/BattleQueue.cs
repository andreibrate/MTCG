using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Http
{
    public class BattleQueue
    {
        private readonly ConcurrentQueue<string> _playerQueue = new();

        public bool TryPairPlayers(string token, out string? opponent)
        {
            if (_playerQueue.TryDequeue(out var dequeuedOpponent))
            {
                opponent = dequeuedOpponent;
                return true;    // Opponent found
            }

            _playerQueue.Enqueue(token);
            opponent = null;    // No opponent available
            return false;
        }
    }
}
