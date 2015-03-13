package fr.polytech.ai;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.net.URL;

import chesspresso.game.Game;
import chesspresso.pgn.PGNReader;
import chesspresso.pgn.PGNSyntaxError;

public class OpeningBook {
	Game[] openingBook = new Game[121];

	public int size(){
		return openingBook.length;
	}
	
	public OpeningBook(){
		URL url = null;
		try {
			url = this.getClass().getResource("book.pgn");
		} catch (Exception e){
			System.out.println("Opening URL error " + e);
		}

		PGNReader pgnReader = null;
		if (url != null){
			try { 
				File f = new File(url.toURI());
				FileInputStream fis = new FileInputStream(f);
				pgnReader = new PGNReader(fis, "book.pgn");
			} catch (Exception e){
				System.out.println("File Error " + e);
			}
		}

		// hack: we know there are only 120 games in the opening book
		if (pgnReader != null){
			for (int i = 0; i < 120; i++)  {
				Game g;
				try {
					g = pgnReader.parseGame();
					openingBook[i] = g;

				} catch (PGNSyntaxError e) {
					System.out.println("PGN SyntaxError " + e);

					e.printStackTrace();
				} catch (IOException e) {

					System.out.println("IOException " + e);
					e.printStackTrace();
				}
			}


		}
	}

}