using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    int[] pieceVal = { 0, 100, 300, 310, 500, 900, 10000 };
    int mateVal = 99999999;
    int maxDepth = 4;
    Move bestMoveRoot;

    int Evaluate(Board board)
    {
        int sum = 0;

        for (int i = 0; ++i < 7;)
            sum += (board.GetPieceList((PieceType)i, true).Count - board.GetPieceList((PieceType)i, false).Count) * pieceVal[i];

        return sum * (board.IsWhiteToMove ? 1 : -1);
    }
    
    int Search(Board board, int depth, int alpha, int beta, int ply)
    {
        Move[] legalMoves = board.GetLegalMoves();

        if (legalMoves.Length == 0)
            return board.IsInCheck() ? ply - mateVal : 0;

        if (depth == 0)
            return Evaluate(board);

        int recordEval = int.MinValue;

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            int evaluation = -Search(board, depth - 1, -beta, -alpha, ply + 1);
            board.UndoMove(move);

            if (recordEval < evaluation)
            {
                recordEval = evaluation;
                if (ply == 0)
                    bestMoveRoot = move;
            }

            alpha = Math.Max(alpha, recordEval);
            if (alpha >= beta) break;
        }

        return recordEval;
    }

    public Move Think(Board board, Timer timer)
    {
        Search(board, maxDepth, -mateVal, mateVal, 0);

        return bestMoveRoot;
    }
}
