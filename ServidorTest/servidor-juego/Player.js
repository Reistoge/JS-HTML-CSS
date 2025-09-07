const AttackSignal = require('./AttackSignal.js');
class Player{
    
    constructor(name) {
        this.isWinner = false;
        this.name = name;
        this.attackSignals = [];

    }
    
    getName() {
        return `${this.name}`;
    }
    getAttackSignals() {
        return this.attackSignals;
    }
    toString() {
        return `Name: ${this.name}` + ` Attacks: ${this.attackSignals}`;
    }
  
    addAttackSignal(attackSignal) {
        if(this.attackSignals.length == 3){
            this.attackSignals.splice(0,1); // remove the first signal
        }
        this.attackSignals.push(attackSignal); // add the new signal
    }
    useAttackSignal(index){
        if (index >=0 && index < this.attackSignals.length) {
            const aux = this.attackSignals[index]; // Get the third signal
            this.attackSignals.splice(index,1);  // use the signal 
            return aux;                      // Return the removed signal
        }
        return null; // Return null if there are fewer than 3 signals
    }
    clearSignals(){
        this.attackSignals = [];
    }
    
}
module.exports = Player;