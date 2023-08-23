using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    struct TTEntry {
        public ulong key;
        public Move move;
        public int depth, score, bound;
        public TTEntry(ulong _key, Move _move, int _depth, int _score, int _bound) {
            key = _key; move = _move; depth = _depth; score = _score; bound = _bound;
        }
    }

    private TTEntry[] tt = new TTEntry [0x400000];
    
    int[] pieceVal = { 0, 100, 300, 310, 500, 900, 10000 };
    int mateVal = 99999999;
    Move bestMoveRoot;
    
    int Evaluate(Board board)
    {
        int sum = 0;

        for (int i = 0; ++i < 7;)
            sum += (board.GetPieceList((PieceType)i, true).Count - board.GetPieceList((PieceType)i, false).Count) * pieceVal[i];

        return sum * (board.IsWhiteToMove ? 1 : -1);
    }
    
    int Search(Board board, Timer timer, int alpha, int beta, int depth, int ply)
    {
        ulong key = board.ZobristKey;
        
        if (ply > 0 && board.IsRepeatedPosition()) return 0;
        
        TTEntry entry = tt[key & 0x3FFFFF];
        
        if(ply > 0 && entry.key == key && entry.depth >= depth && (
               entry.bound == 3 // exact score
               || entry.bound == 2 && entry.score >= beta // lower bound, fail high
               || entry.bound == 1 && entry.score <= alpha // upper bound, fail low
           )) return entry.score;
        
        Move[] moves = board.GetLegalMoves();
        
        if (moves.Length == 0)
            return board.IsInCheck() ? ply - mateVal : 0;

        if (depth == 0)
            return Evaluate(board);
        
        int[] scores = new int[moves.Length];
        
        for(int i = 0; i < moves.Length; i++) {
            Move move = moves[i];
            if(move == entry.move) scores[i] = mateVal;
        }

        int bestEval = int.MinValue;
        Move bestMove = Move.NullMove;
        int oldAlpha = alpha;

        for(int i = 0; i < moves.Length; i++)
        {
            for(int j = i + 1; j < moves.Length; j++) {
                if(scores[j] > scores[i])
                    (scores[i], scores[j], moves[i], moves[j]) = (scores[j], scores[i], moves[j], moves[i]);
            }
            
            Move move = moves[i];
            board.MakeMove(move);
            int evaluation = -Search(board, timer, -beta, -alpha, depth - 1, ply + 1);
            board.UndoMove(move);
            
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return mateVal;
            
            if (evaluation > bestEval)
            {
                bestEval = evaluation;
                bestMove = move;
                if (ply == 0)
                    bestMoveRoot = move;
            }
            alpha = Math.Max(alpha, bestEval);
            if (alpha >= beta) break;
        }
        
        tt[key & 0x3FFFFF] = new TTEntry(key, bestMove, depth, bestEval, bestEval >= beta ? 2 : bestEval > oldAlpha ? 1 : 3);

        return bestEval;
    }

    public Move Think(Board board, Timer timer)
    {
        bestMoveRoot = Move.NullMove;
        for (int depth = 1; depth <= 50; depth++)
        {
            Search(board, timer, -mateVal, mateVal, depth, 0);

            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) break;
        }
        return bestMoveRoot.IsNull ? board.GetLegalMoves()[0] : bestMoveRoot;
    }
}
