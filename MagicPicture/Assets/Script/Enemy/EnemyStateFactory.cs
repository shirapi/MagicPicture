﻿using UnityEngine;

class EnemyStateFactory
{
    private GameObject obj;
    private Finder finder;
    private float dashSpeed;
    private float dashRotSpeed;
    private float breakInterval;
    private int actionNum;

    public EnemyStateFactory(GameObject obj, Finder finder, 
        float dashSpeed, 
        float dashRotSpeed,
        float breakInterval, int actionNum)
    {
        this.obj = obj;
        this.finder = finder;
        this.dashSpeed = dashSpeed;
        this.dashRotSpeed = dashRotSpeed;
        this.actionNum = actionNum;
        this.breakInterval = breakInterval;
    }

    public EnemyStateBase Create(EnemyAI.STATE state)
    {
        EnemyStateBase ret = null;

        switch (state)
        {
            case EnemyAI.STATE.ATTACK:
                ret = new EnemyStateAttack(this.obj, this.finder, this.dashSpeed, dashRotSpeed);
                break;

            case EnemyAI.STATE.BREAKTIME:
                ret = new EnemyStateBreaktime(this.obj, this.finder, this.breakInterval);
                break;

            case EnemyAI.STATE.SEARCH:
                ret = new EnemyStateSearch(this.obj, this.finder, this.actionNum);
                break;
        }

        return ret;
    }
}