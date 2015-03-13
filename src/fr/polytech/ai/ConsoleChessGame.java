package fr.polytech.ai;
import java.util.Scanner;

import chesspresso.Chess;

public class ConsoleChessGame {
	
	private static final int PIXELS_PER_SQUARE = 64;
	
	ChessGame game;
	
	public ConsoleChessGame() {
		this.game = new ChessGame();
		ChessAI blackPlayer =  new AlphaBetaAI(2);
		//ChessAI whitePlayer =  new RandomAI();

		displayBoard();
		for (int i = 0; i < 30; i++) {
			System.out.println("Coup "+i);
			//game.doMove(whitePlayer.getMove(game.position));
			short move = 0;
			while (move == 0) {
				Scanner terminalInput = new Scanner(System.in);
				String playerInput = terminalInput.nextLine();
				move = findMove(playerInput);
			}
			game.doMove(move);
			displayBoard();
			game.doMove(blackPlayer.getMove(game.position));
			displayBoard();
		}
	}



	private void displayBoard() {
		System.out.println();
		System.out.println("----------------");
		for (int j = 0; j < 8; j++) {
			System.out.print("| ");
			for (int k = 0; k < 8; k++) {
				String stone = Chess.stoneToChar(game.getStone(k, j))+"";
				if (Chess.stoneToColor(game.getStone(k, j)) == Chess.WHITE) {
					stone = stone.toLowerCase();
				}
				System.out.print(stone+" ");
			}
			System.out.println(" |");
		}
		System.out.println("----------------");
		System.out.println();
	}
	

	
	private short findMove(String text) {
		if (text != null & text != "") {
			int fromSqi = Chess.strToSqi(text.charAt(0), text.charAt(1));
			int toSqi = Chess.strToSqi(text.charAt(2), text.charAt(3));
			short move = game.findMove(fromSqi, toSqi);
			return move;
		}
		return 0;
	}
}
