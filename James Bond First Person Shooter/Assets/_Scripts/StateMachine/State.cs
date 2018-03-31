using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour {

    protected Character character;

    public abstract void StateUpdate();

    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }

    public State(Character character)
    {
        this.character = character;
    }
}

public class Character
{

}
