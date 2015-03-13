package fr.polytech.ai;
public class ChessMove {
	
	public short actualMove;
	public int value;
	
	public ChessMove(short actualMove, int value){
		this.actualMove = actualMove;
		this.value = value;
	}
	
	// set the value of the chess
	public void setValue(int value){
		this.value = value;
	}
	
	public double getValue(){
		return value;
	}
	
	// set the move of the chess
	public void setMove(short move){
		this.actualMove = move;
	}
	
}