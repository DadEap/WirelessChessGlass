package fr.polytech.ai;

import java.util.Random;

import chesspresso.position.Position;

public class RandomAI implements ChessAI {
	public short getMove(Position position) {
		short [] moves = position.getAllMoves();
		System.out.println(position.getToPlay());
		
		// n must be positive in case Random() is not positive
		if (moves.length != 0){
			short move = moves[new Random().nextInt(moves.length)];
			return move;
		} else { 
			return (short) 0;
		}
		
	}
}
