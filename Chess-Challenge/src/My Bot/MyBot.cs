using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Random random = new();
        Move[] moves = board.GetLegalMoves();
        Move best = moves[random.Next(moves.Length)];
        double bestScore = whiteSign(board) * -double.MaxValue;
        foreach (var move in moves)
        {
            double score = evaluate(move, board, 3);
            if (!board.IsWhiteToMove ^ score > bestScore)
            {
                best = move;
                bestScore = score;
            }
        }
            
        Console.WriteLine("Depth 0: {0}, Depth 3: {1}", 
            Math.Round(evaluate(board), 2), 
            Math.Round(bestScore, 2));
        return best;
    }

    double checkCaptureAttack(Move move, Board board, int depth) 
    {
        bool trade = move.IsCapture && board.SquareIsAttackedByOpponent(move.TargetSquare);
        bool inCheck = board.IsInCheck();
        board.MakeMove(move);
        if (board.IsInCheckmate()) 
        {
            board.UndoMove(move);
            return 1000;
        }
        double max = 0;
        if (depth > 0) 
            foreach (var oppMove in board.GetLegalMoves()) 
            {
                var depthUse = inCheck || board.IsInCheck() || trade ? 1 : 100;
                if (depthUse >= depth)
                {
                    var score = checkCaptureAttack(oppMove, board, depth - depthUse);
                    if (score > max) max = score;
                }
            }
            max -= whiteSign(board) * evaluate(board);
        board.UndoMove(move);
        return Math.Abs(evaluate(board.GetPiece(move.TargetSquare))) 
            - whiteSign(board) * evaluate(board) - max;
    }

    double evaluate(Move move, Board board, int depth) 
    {
        board.MakeMove(move);

        board.UndoMove(move);
        return evaluate(board);
    }

    double evaluate(Board board)
    {
        if (board.IsInCheckmate())
            return whiteSign(board) * -1000;
        if (board.IsDraw())
            return 0;
        double sum = 0;
            if (board.IsInCheck())
                sum += whiteSign(board) * -0.5;
            for (int i = 0; i < 64; i++)
                sum += evaluate(board.GetPiece(new Square(i)));
            board.ForceSkipTurn();
            sum += whiteSign(board) * 0.1 * board.GetLegalMoves().Length;
            board.UndoSkipTurn();
            sum += whiteSign(board) * 0.1 * board.GetLegalMoves().Length;
            return sum;
    }

    double evaluate(Piece piece)
    {
        int sign = piece.IsWhite ? 1 : -1;
        if (piece.IsPawn) return sign + piece.Square.Rank / 10;
        if (piece.IsKnight) return sign * 2.9;
        if (piece.IsBishop) return sign * 3;
        if (piece.IsRook) return sign * 5;
        if (piece.IsQueen) return sign * 9;
        return 0;
    }

    int whiteSign(Board board) 
    {
        return board.IsWhiteToMove ? 1 : -1;
    }
}