// The AttackSignal class represents a signal for an attack in a game, 
// with varying levels of strength (LOW, MEDIUM, HIGH) defined as static properties.
class AttackSignal {

    constructor(attackStrength = AttackSignal.attackStrength.LOW) {
        this.type = attackStrength;
    }

    static attackStrength = {
        LOW: 1,
        MEDIUM: 2,
        HIGH: 3
    };

}
// Removed duplicate attackStrength object as it is now part of the class.
