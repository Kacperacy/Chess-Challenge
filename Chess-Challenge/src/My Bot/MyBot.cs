using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    private int[,] positionEval =
    {
        {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5, -5,-10,  0,  0,-10, -5,  5,
            5, 10, 10,-20,-20, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0
        },
        {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50,
        },
        {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
        },
        {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            0,  0,  0,  5,  5,  0,  0,  0
        },
        {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -5,  0,  5,  5,  5,  5,  0, -5,
            0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        },
        {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
            20, 20,  0,  0,  0,  0, 20, 20,
            20, 30, 10,  0,  0, 10, 30, 20
        },
        {
            -50,-40,-30,-20,-20,-30,-40,-50,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-30,  0,  0,  0,  0,-30,-30,
            -50,-30,-30,-30,-30,-30,-30,-50
        }
    };

    int[] pieceVal = { 0, 100, 300, 310, 500, 900, 10000 };
    int massiveNum = 99999999;
    int startingDepth = 4;
    int captureDepth = 1;
    int totalDepth;
    Move bestMoveRoot = Move.NullMove;

    int NegaMax(Board board, int depth, int alpha, int beta, int color)
    {
        if (board.IsDraw())
            return 0;

        Move[] legalMoves = depth > captureDepth ? board.GetLegalMoves() : board.GetLegalMoves(true);

        if (depth == 0 || legalMoves.Length == 0)
        {
            int sum = 0;

            if (board.IsInCheckmate())
                return -massiveNum;

            for (int i = 0; ++i < 7;)
            {
                PieceList white = board.GetPieceList((PieceType)i, true);
                PieceList black = board.GetPieceList((PieceType)i, false);

                sum += (white.Count - black.Count) * pieceVal[i];

                if (i == 6 && board.GetAllPieceLists().Sum(pieceList => pieceList.Count) < 16)
                {
                    sum += white.Sum(piece => positionEval[6, piece.Square.Index]);
                    sum -= black.Sum(piece => positionEval[6, 63 - piece.Square.Index]);
                }
                else
                {
                    sum += white.Sum(piece => positionEval[i - 1, piece.Square.Index]);
                    sum -= black.Sum(piece => positionEval[i - 1, 63 - piece.Square.Index]);
                }
            }

            return color * sum;
        }

        int recordEval = int.MinValue;

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            int evaluation = -NegaMax(board, depth - 1, -beta, -alpha, -color);
            board.UndoMove(move);

            if (recordEval < evaluation)
            {
                recordEval = evaluation;
                if (depth == totalDepth)
                    bestMoveRoot = move;
            }

            alpha = Math.Max(alpha, recordEval);
            if (alpha >= beta) break;
        }

        return recordEval;
    }

    public Move Think(Board board, Timer timer)
    {
        totalDepth = startingDepth + captureDepth;

        NegaMax(board, totalDepth, -massiveNum, massiveNum, board.IsWhiteToMove ? 1 : -1);

        return bestMoveRoot;
    }
}
