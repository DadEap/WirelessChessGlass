package fr.polytech.ai;

import chesspresso.Chess;
import chesspresso.move.IllegalMoveException;
import chesspresso.position.Position;

// AI that implements basic minimax algorithm
public class MiniMaxAI implements ChessAI {
	
	int DEPTH; // basic depth that AI can search at
	int AI_PLAYER;
	
	public MiniMaxAI(int depth) {
		DEPTH = depth;
	}
	
	public short getMove(Position position) {
		AI_PLAYER = position.getToPlay();
		
		// start the game-tree search
		ChessMove bestMove = maxVal(position, 6);
		
		for(int i = 1; i < DEPTH; i++){
			ChessMove possBestMove = maxVal(position, i);
			if (bestMove.value < possBestMove.value){
				bestMove = possBestMove;
			}
		}
		
		return bestMove.actualMove;
	}
	
	private ChessMove maxVal(Position position, int depth){
		if(position.isMate() || depth == 0){
			return new ChessMove((short) 0, terminalTest(position));
		}
		
		// maximizer wants to get the highest possible
		ChessMove bestMove = new ChessMove((short) 0, Integer.MIN_VALUE);
		for (short possMove: position.getAllMoves()){
			try {
				position.doMove(possMove);
				
				ChessMove possBestMove = minVal(position, depth - 1);
				if (possBestMove.value > bestMove.value){
					bestMove.setValue(possBestMove.value);
					bestMove.setMove(possMove);
				}
				position.undoMove();
			} catch (IllegalMoveException e){
				e.printStackTrace();
			}
		}
		return bestMove;
	}
	
	private ChessMove minVal(Position position, int depth){
		if(position.isMate() || depth == 0){
			return new ChessMove((short) 0, terminalTest(position));
		}
		
		// minimizer wants to get the lowest possible
		ChessMove bestMove = new ChessMove((short) 0, Integer.MAX_VALUE);
		for (short possMove: position.getAllMoves()){
			try {
				position.doMove(possMove);
				ChessMove possBestMove = maxVal(position, depth-1);
				if (possBestMove.value < bestMove.value){
					bestMove.setValue(possBestMove.value);
					bestMove.setMove(possMove);
				}
				position.undoMove();
			} catch (IllegalMoveException e){
				e.printStackTrace();
			}
		}

		return bestMove;
		
	}
	
	private int evalFunc(Position position){
		if (position.getToPlay() == AI_PLAYER){
			return position.getMaterial();
		} else {
			return -1 * position.getMaterial();
		}
	}
	
	
	
	// tests whether position is in terminal state (win, loss, draw)
	// returns 1 if win;
	// returns 0 if draw;
	// returns -1 if loss;
	// returns null if not terminal state
	private int terminalTest(Position position){
		// Position is not checkmate but >50 moves (in isTerminal)
		if (position.isStaleMate()){
			return 0;
		}
		
		// if current player is in check and can't move, it's a checkmate
		if (position.isMate()){
			if (AI_PLAYER == position.getToPlay()){
				
				// If the position is checkmate, then AI has lost
					return Integer.MIN_VALUE; //loss
			} else {
				// If the position is checkmate, then AI has won (not AI's turn)
					return Integer.MAX_VALUE; //win
			}
		} else {
			return evalFunc(position);
		}
	}
	
}
