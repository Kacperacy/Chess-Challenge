using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
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
        if (ply > 0 && board.IsRepeatedPosition()) return 0;
        
        Move[] legalMoves = board.GetLegalMoves();

        if (ply == 0 && !bestMoveRoot.IsNull)
        {
            for (int i = 0; i < legalMoves.Length; i++)
            {
                if(legalMoves[i] == bestMoveRoot)
                {
                    legalMoves[i] = legalMoves[0];
                    legalMoves[0] = bestMoveRoot;
                }
            }
        }
        
        if (legalMoves.Length == 0)
            return board.IsInCheck() ? ply - mateVal : 0;

        if (depth == 0)
            return Evaluate(board);

        int bestEval = int.MinValue;

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            int evaluation = -Search(board, timer, -beta, -alpha, depth - 1, ply + 1);
            board.UndoMove(move);
            
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return mateVal;
            
            if (evaluation > bestEval)
            {
                bestEval = evaluation;
                if (ply == 0)
                    bestMoveRoot = move;
            }
            alpha = Math.Max(alpha, bestEval);
            if (alpha >= beta) break;
        }

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
