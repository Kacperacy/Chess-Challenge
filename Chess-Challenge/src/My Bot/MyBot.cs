using System;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    private int[] pawnEval =
    {
        0, 0, 0, 0, 0, 0, 0, 0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
        5, 5, 10, 25, 25, 10, 5, 5,
        0, 0, 0, 20, 20, 0, 0, 0,
        5, -5, -10, 0, 0, -10, -5, 5,
        5, 10, 10, -20, -20, 10, 10, 5,
        0, 0, 0, 0, 0, 0, 0, 0
    };

    private int[] knightEval =
    {
        -50, -40, -30, -30, -30, -30, -40, -50,
        -40, -20, 0, 0, 0, 0, -20, -40,
        -30, 0, 10, 15, 15, 10, 0, -30,
        -30, 5, 15, 20, 20, 15, 5, -30,
        -30, 0, 15, 20, 20, 15, 0, -30,
        -30, 5, 10, 15, 15, 10, 5, -30,
        -40, -20, 00, 5, 05, 00, -20, -40,
        -50, -40, -30, -30, -30, -30, -40, -50
    };

    private int[] bishopEval =
    {
        -20, -10, -10, -10, -10, -10, -10, -20,
        -10, 0, 0, 0, 0, 0, 0, -10,
        -10, 0, 5, 10, 10, 5, 0, -10,
        -10, 5, 5, 10, 10, 5, 5, -10,
        -10, 0, 10, 10, 10, 10, 0, -10,
        -10, 10, 10, 10, 10, 10, 10, -10,
        -10, 5, 0, 0, 0, 0, 5, -10,
        -20, -10, -10, -10, -10, -10, -10, -20
    };

    private int[] rookEval =
    {
        0, 0, 0, 0, 0, 0, 0, 0,
        5, 10, 10, 10, 10, 10, 10, 5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        0, 0, 0, 5, 5, 0, 0, 0
    };

    private int[] queenEval =
    {
        -20, -10, -10, -5, -5, -10, -10, -20,
        -10, 0, 0, 0, 0, 0, 0, -10,
        -10, 0, 5, 5, 5, 5, 0, -10,
        -5, 0, 5, 5, 5, 5, 0, -5,
        0, 0, 5, 5, 5, 5, 0, -5,
        -10, 5, 5, 5, 5, 5, 0, -10,
        -10, 0, 5, 0, 0, 0, 0, -10,
        -20, -10, -10, -5, -5, -10, -10, -20
    };
    
    public Move Think(Board board, Timer timer)
    {
        int depth = 3;
        int bestMove = -9999;
        
        var possibleMoves = board.GetLegalMoves();
        Move bestMoveFound = possibleMoves[0];

        foreach (var move in possibleMoves)
        {
            board.MakeMove(move);

            var moveValue = MiniMax(depth, board, -10000, 10000, false);
            
            board.UndoMove(move);

            if (moveValue >= bestMove)
            {
                bestMove = moveValue;
                bestMoveFound = move;
            }
        }

        return bestMoveFound;
    }

    private int MiniMax(int depth, Board board, int alpha, int beta, bool isMaximisingPlayer)
    {
        if (board.IsInCheckmate())
        {
            return isMaximisingPlayer ? -9999 : 9999;
        }

        if (board.IsDraw())
        {
            return 0;
        }
        
        if (depth == 0)
        {
            return Evaluate(board, isMaximisingPlayer);
        }

        var possibleMoves = board.GetLegalMoves();

        if (isMaximisingPlayer)
        {
            var bestMove = -9999;
            foreach (var move in possibleMoves)
            {
                board.MakeMove(move);
                bestMove = Math.Max(bestMove, MiniMax(depth - 1, board, alpha, beta, !isMaximisingPlayer));
                board.UndoMove(move);
                alpha = Math.Max(alpha, bestMove);
                if (beta <= alpha)
                {
                    return bestMove;
                }
            }
            return bestMove;
        }
        else
        {
            var bestMove = 9999;
            foreach (var move in possibleMoves)
            {
                board.MakeMove(move);
                bestMove = Math.Min(bestMove, MiniMax(depth - 1, board, alpha, beta, !isMaximisingPlayer));
                board.UndoMove(move);
                beta = Math.Min(beta, bestMove);
                if (beta <= alpha)
                {
                    return bestMove;
                }
            }
            return bestMove;
        }
    }

    private int Evaluate(Board board, bool isMaximisingPlayer)
    {
        var pieceLists = board.GetAllPieceLists();
        int whiteEvaluation = 0, blackEvaluation = 0;

        whiteEvaluation += pieceLists[0].Sum(piece => pawnEval[piece.Square.Index] + pieceValues[1]);
        whiteEvaluation += pieceLists[1].Sum(piece => knightEval[piece.Square.Index] + pieceValues[2]);
        whiteEvaluation += pieceLists[2].Sum(piece => bishopEval[piece.Square.Index] + pieceValues[3]);
        whiteEvaluation += pieceLists[3].Sum(piece => rookEval[piece.Square.Index] + pieceValues[4]);
        whiteEvaluation += pieceLists[4].Sum(piece => queenEval[piece.Square.Index] + pieceValues[5]);

        blackEvaluation += pieceLists[6].Sum(piece => pawnEval[63 - piece.Square.Index] + pieceValues[1]);
        blackEvaluation += pieceLists[7].Sum(piece => knightEval[63 - piece.Square.Index] + pieceValues[2]);
        blackEvaluation += pieceLists[8].Sum(piece => bishopEval[63 - piece.Square.Index] + pieceValues[3]);
        blackEvaluation += pieceLists[9].Sum(piece => rookEval[63 - piece.Square.Index] + pieceValues[4]);
        blackEvaluation += pieceLists[10].Sum(piece => queenEval[63 - piece.Square.Index] + pieceValues[5]);

        if (isMaximisingPlayer)
        {
            return board.IsWhiteToMove ? whiteEvaluation - blackEvaluation : blackEvaluation - whiteEvaluation;
        }
        else
        {
            return board.IsWhiteToMove ? blackEvaluation - whiteEvaluation : whiteEvaluation - blackEvaluation;
        }
    }
}