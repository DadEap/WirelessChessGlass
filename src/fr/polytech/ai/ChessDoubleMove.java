package fr.polytech.ai;
public class ChessDoubleMove {
	
	public short actualMove;
	public double value;
	
	public ChessDoubleMove(short actualMove, double value){
		this.actualMove = actualMove;
		this.value = value;
	}
	
	// set the value of the chess
	private void setValue(double value){
		this.value = value;
	}
	
	private double getValue(){
		return value;
	}
	
	// set the move of the chess
	private void setMove(short move){
		this.actualMove = move;
	}
	
}