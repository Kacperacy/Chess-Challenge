using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    // Pawn, Knight, Bishop, Rook, Queen, King 
    int[] pieceVal = { 0, 100, 300, 310, 500, 900, 10000 };

    #region TTEntry
    // 0 = Invalid
    // 1 = Upper bound
    // 2 = Lower bound
    // 3 = Exact score
    struct TTEntry {
        public ulong key;
        public Move move;
        public int depth, score, bound;
        public TTEntry(ulong _key, Move _move, int _depth, int _score, int _bound) {
            key = _key; move = _move; depth = _depth; score = _score; bound = _bound;
        }
    }

    TTEntry[] tt = new TTEntry [0x400000];
    #endregion
    
    // Mate value = 99999
    Move moveRoot;
    private readonly int[] moveScores = new int[218];
    
    #region Evaluate()
    int Evaluate(Board board)
    {
        int sum = 0;

        for (int i = 0; ++i < 7;)
            sum += (board.GetPieceList((PieceType)i, true).Count - board.GetPieceList((PieceType)i, false).Count) * pieceVal[i];

        return sum * (board.IsWhiteToMove ? 1 : -1);
    }
    #endregion
    
    #region Search()
    int Search(Board board, Timer timer, int alpha, int beta, int depth, int ply)
    {
        bool isCheck = board.IsInCheck(), isRoot = ply++ == 0;
        
        if (!isRoot && board.IsRepeatedPosition()) return 0;
        
        ulong key = board.ZobristKey;
        TTEntry entry = tt[key & 0x3FFFFF];
        
        // Transposition table lookup -> Found a valid entry for this position
        // Avoid retrieving mate scores from the TT since they aren't accurate to the ply
        if(!isRoot && entry.key == key && entry.depth >= depth && Math.Abs(entry.score) < 50000 && (
               entry.bound == 3 // exact score
               || entry.bound == 2 && entry.score >= beta // lower bound, fail high
               || entry.bound == 1 && entry.score <= alpha // upper bound, fail low
           )) return entry.score;
        
        // Check extension
        if(isCheck)
            depth++;
        
        if (depth == 0)
            return Evaluate(board);

        Span<Move> moveSpan = stackalloc Move[218];
        board.GetLegalMovesNonAlloc(ref moveSpan);
        
        if (moveSpan.IsEmpty)
            return isCheck ? ply - 99999 : 0;

        int movesScored = 0;

        // Order moves in reverse order -> negative values are ordered higher hence the flipped values
        foreach (var move in moveSpan)
            moveScores[movesScored++] = -(
                // Stored move
                move == entry.move ? 9_000_000 :
                // MVV LVA
                move.IsCapture ? 1_000_000 * (int)move.CapturePieceType - (int)move.MovePieceType :
                // Return 0
                0
            );

        moveScores.AsSpan(0, moveSpan.Length).Sort(moveSpan);
        
        int bestEval = int.MinValue, oldAlpha = alpha;
        Move bestMove = Move.NullMove;

        for(int i = 0; i < moveSpan.Length; i++)
        {
            Move move = moveSpan[i];
            board.MakeMove(move);
            int evaluation = -Search(board, timer, -beta, -alpha, depth - 1, ply);
            board.UndoMove(move);
            
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 99999;
            
            if (evaluation > bestEval)
            {
                bestEval = evaluation;
                bestMove = move;
                if (ply == 0)
                    moveRoot = move;
            }
            alpha = Math.Max(alpha, bestEval);
            if (alpha >= beta) break;
        }
        
        tt[key & 0x3FFFFF] = new TTEntry(key, bestMove, depth, bestEval, bestEval >= beta ? 2 : bestEval > oldAlpha ? 1 : 3);

        return bestEval;
    }
    #endregion

    #region Think()
    public Move Think(Board board, Timer timer)
    {
        for (int depth = 1; depth <= 50; depth++)
        {
            Search(board, timer, -99999, 99999, depth, 0);

            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) break;
        }
        return moveRoot.IsNull ? board.GetLegalMoves()[0] : moveRoot;
    }
    #endregion
}
