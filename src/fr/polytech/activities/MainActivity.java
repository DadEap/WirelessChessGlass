package fr.polytech.activities;

import java.util.Scanner;

import chesspresso.Chess;
import android.app.Activity;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.inputmethod.EditorInfo;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.TextView.OnEditorActionListener;
import fr.polytech.activities.R;
import fr.polytech.ai.AlphaBetaAI;
import fr.polytech.ai.ChessAI;
import fr.polytech.ai.ChessGame;

public class MainActivity extends Activity {

	ChessGame game;
	ChessAI blackPlayer;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		
		this.game = new ChessGame();
		// The user is the white player
		// The computer is the black player
		this.blackPlayer =  new AlphaBetaAI(2);

		EditText nextTurnText = (EditText) findViewById(R.id.next_turn);
		TextView chessboard = (TextView) findViewById(R.id.chessboard_text);
		chessboard.setText(displayBoard());
		
		nextTurnText.setOnEditorActionListener(new OnEditorActionListener() {
		    @Override
		    public boolean onEditorAction(TextView v, int actionId, KeyEvent event) {
		    	TextView chessboard = (TextView) findViewById(R.id.chessboard_text);
		    	String moveText = v.getText().toString();
		    	if (moveText.length() == 4) {
					short move = findMove(moveText);
			        if (move != 0) {
			        	game.doMove(move);
			        	chessboard.setText(displayBoard());
						game.doMove(blackPlayer.getMove(game.position));
						chessboard.setText(displayBoard());
						v.setText("");
			        }
			        return true;
				}
				return false;
		    }
		});
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// Handle action bar item clicks here. The action bar will
		// automatically handle clicks on the Home/Up button, so long
		// as you specify a parent activity in AndroidManifest.xml.
		int id = item.getItemId();
		if (id == R.id.action_settings) {
			return true;
		}
		return super.onOptionsItemSelected(item);
	}
	
	private String displayBoard() {
		String chessboard = "--------------------\n";
		for (int j = 7; j >= 0; j--) {
			chessboard += "| ";
			for (int k = 0; k < 8; k++) {
				String stone = Chess.stoneToChar(game.getStone(k, j))+"";
				if (Chess.stoneToColor(game.getStone(k, j)) == Chess.WHITE) {
					stone = stone.toLowerCase();
				}
				chessboard += stone+" ";
			}
			chessboard += " |\n";
		}
		chessboard += "--------------------\n";
		return chessboard;
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
