﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBasedMover : MonoBehaviour
{
    public Animator animator;
    public FollowDwarf lampy;
    public MapGen world;

    public float moveSpeed = 1f;
    public float climbingDifficulty = 2f;
    public float fallSpeedMultiplier = 1f;
    private bool canMove = true, moving = false, m_FacingRight = true;
    public bool isFalling = false;
    public float setMoveCooldown = 1f;
    private float moveCooldown = 0f;
    public float tileDifficultyMultiplier = 1f, climbingDifficultyMultiplier = 1f, strengthMultiplier = 1f;
    public bool isDestroyed = false;

    private Vector2 touchOrigin = -Vector2.one;

    public Vector3 targetPos;

    void Move()
    {
        if (moveCooldown <= 0f)
        {
            float horizontal, vertical;

           #if UNITY_STANDALONE || UNITY_WEBPLAYER

            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            #elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

            //Check if Input has registered more than zero touches
            if (Input.touchCount > 0)
            {
                //Store the first touch detected.
                Touch myTouch = Input.touches[0];

                //Check if the phase of that touch equals Began
                if (myTouch.phase == TouchPhase.Began)
                {
                    //If so, set touchOrigin to the position of that touch
                    touchOrigin = myTouch.position;
                }

                //If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
                else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
                {
                    //Set touchEnd to equal the position of this touch
                    Vector2 touchEnd = myTouch.position;

                    //Calculate the difference between the beginning and end of the touch on the x axis.
                    float x = touchOrigin.x - Screen.width / 2;

                    //Calculate the difference between the beginning and end of the touch on the y axis.
                    float y = touchOrigin.y - Screen.height / 2;

                    //Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
                    touchOrigin.x = -1;

                    //Check if the difference along the x axis is greater than the difference along the y axis.
                    if (Mathf.Abs(x) > Mathf.Abs(y))
                        //If x is greater than zero, set horizontal to 1, otherwise set it to -1
                        horizontal = x > 0 ? 1 : -1;
                    else
                        //If y is greater than zero, set horizontal to 1, otherwise set it to -1
                        vertical = y > 0 ? 1 : -1;
                }
            }

            #endif //End of mobile platform dependendent compilation section started above with #elif

            if (horizontal != 0 || vertical != 0)
            {
                moving = true;
                canMove = false;

                if (horizontal != 0)
                {
                    vertical = 0;
                }

                if (m_FacingRight && horizontal < 0 || !m_FacingRight && horizontal > 0)
                {
                    Flip();
                }


                
                targetPos += new Vector3(horizontal, vertical, 0f);

                GameObject moveTile = world.getTile(targetPos);
                

                if (moveTile == null)
                {
                    //Dwarf is moving into empty space

                    if (world.getTile(targetPos + new Vector3(0f, -1f, 0f)) != null)
                    {
                        //There is a block below the target position, movement is good

                        if (vertical < 0 && world.getTile(targetPos + new Vector3(-1f, 0f, 0f)) == null && world.getTile(targetPos + new Vector3(1f, 0f, 0f)) == null)
                        {
                            //dwarf is falling down one block
                            isFalling = true;
                            fallSpeedMultiplier = 3f;
                            animator.SetBool("isFalling", true);
                        }
                    }
                    else
                    {
                        //There is no block below the target position, check if climbing is possible

                        if (world.getTile(targetPos + new Vector3(-1f, 0f, 0f)) != null && world.getTile(targetPos + new Vector3(1f, 0f, 0f)) != null)
                        {
                            //There is both a block to the left and to the right of the target position, use the direction that the dwarf is facing to climb
                            climbingDifficultyMultiplier = climbingDifficulty;
                        }
                        else if (world.getTile(targetPos + new Vector3(-1f, 0f, 0f)) != null)
                        {
                            //There is a block to the left of the target position, climbing on the left is possible
                            if (m_FacingRight)
                            {
                                Flip();
                            }
                            climbingDifficultyMultiplier = climbingDifficulty;
                        }
                        else if (world.getTile(targetPos + new Vector3(1f, 0f, 0f)) != null)
                        {
                            //There is a block to the right of the target position, climbing on the right is possible
                            if (!m_FacingRight)
                            {
                                Flip();
                            }
                            climbingDifficultyMultiplier = climbingDifficulty;
                        }
                        else
                        {
                            //There is no block on either side of the target position, check if the dwarf can climb up/over or down/under a tile
                            
                            if (vertical > 0)
                            {
                                if (world.getTile(transform.position + new Vector3(-1f, 0f, 0f)) != null && world.getTile(transform.position + new Vector3(1f, 0f, 0f)) != null)
                                {
                                    //There is both a block to the left and to the right of the current position, use facing direction to climb up and over
                                    if (m_FacingRight)
                                    {
                                        targetPos += new Vector3(1f, 0f, 0f);
                                    }
                                    else
                                    {
                                        targetPos += new Vector3(-1f, 0f, 0f);
                                    }
                                    climbingDifficultyMultiplier = climbingDifficulty;
                                }
                                else if (world.getTile(transform.position + new Vector3(-1f, 0f, 0f)) != null)
                                {
                                    //There is a block to the left of the current position, climb up and over to the left
                                    if (m_FacingRight)
                                    {
                                        Flip();
                                    }
                                    targetPos += new Vector3(-1f, 0f, 0f);
                                    climbingDifficultyMultiplier = climbingDifficulty;
                                }
                                else if (world.getTile(transform.position + new Vector3(1f, 0f, 0f)) != null)
                                {
                                    //There is a block to the right of the current position, climb up and over to the right
                                    if (!m_FacingRight)
                                    {
                                        Flip();
                                    }
                                    targetPos += new Vector3(1f, 0f, 0f);
                                    climbingDifficultyMultiplier = climbingDifficulty;
                                }
                                else if (world.getTile(targetPos + new Vector3(0f, 1f, 0f)) != null)
                                {
                                    //There is a block above the target position, dwarf can jump and attach to ceiling
                                    animator.SetBool("isJumping", true);
                                }
                                else
                                {
                                    //movement not possible
                                    moving = false;
                                    canMove = true;

                                    return;
                                }
                            }
                            else if (horizontal < 0)
                            {
                                if (m_FacingRight)
                                {
                                    Flip();
                                }

                                if (world.getTile(transform.position + new Vector3(0f, -1f, 0f)) != null)
                                {
                                    //There is a block below the current position, climb down and to the left
                                    targetPos += new Vector3(0f, -1f, 0f);
                                    climbingDifficultyMultiplier = climbingDifficulty;
                                }
                                else if (world.getTile(targetPos + new Vector3(0f, 1f, 0f)) != null)
                                {
                                    //There is a block above the target position, climb along the ceiling
                                    climbingDifficultyMultiplier = climbingDifficulty;
                                }
                                else if (world.getTile(transform.position + new Vector3(0f, 1f, 0f)) != null)
                                {
                                    //There is a block above the current position, climb around it
                                    targetPos += new Vector3(0f, 1f, 0f);
                                    climbingDifficultyMultiplier = climbingDifficulty;
                                }
                                else
                                {
                                    //movement not possible
                                    moving = false;
                                    canMove = true;

                                    return;
                                }
                            }
                            else if (horizontal > 0)
                            {
                                if (!m_FacingRight)
                                {
                                    Flip();
                                }

                                if (world.getTile(transform.position + new Vector3(0f, -1f, 0f)) != null)
                                {
                                    //There is a block below the current position, climb down and to the right
                                    targetPos += new Vector3(0f, -1f, 0f);
                                    climbingDifficultyMultiplier = climbingDifficulty;
                                }
                                else if (world.getTile(targetPos + new Vector3(0f, 1f, 0f)) != null)
                                {
                                    //There is a block above the target position, climb along the ceiling
                                    climbingDifficultyMultiplier = climbingDifficulty;
                                }
                                else if (world.getTile(transform.position + new Vector3(0f, 1f, 0f)) != null)
                                {
                                    //There is a block above the current position, climb around it
                                    targetPos += new Vector3(0f, 1f, 0f);
                                    climbingDifficultyMultiplier = climbingDifficulty;
                                }
                                else
                                {
                                    //movement not possible
                                    moving = false;
                                    canMove = true;

                                    return;
                                }
                            }
                            else if (vertical < 0)
                            {
                                //dwarf is falling
                                isFalling = true;
                                fallSpeedMultiplier = 3f;
                                animator.SetBool("isFalling", true);
                            }
                            else
                            {
                                //movement not possible
                                moving = false;
                                canMove = true;

                                return;
                            }

                        }
                    }
                }
                else
                {
                    //Digging attempted
                    bool validDig = true;

                    if (world.getTile(targetPos + new Vector3(-1f, 0f, 0f)) == null && world.getTile(targetPos + new Vector3(1f, 0f, 0f)) == null)
                    {
                        if (world.getTile(targetPos + new Vector3(0f, -1f, 0f)) == null)
                        {
                            if (vertical > 0)
                            {
                                //climb around block depending on direction facing
                                if (m_FacingRight)
                                {
                                    targetPos += new Vector3(1f, 0f, 0f);
                                    climbingDifficultyMultiplier = climbingDifficulty;

                                    validDig = false;
                                }
                                else
                                {
                                    targetPos += new Vector3(-1f, 0f, 0f);
                                    climbingDifficultyMultiplier = climbingDifficulty;

                                    validDig = false;
                                }
                            }
                            else if (vertical < 0)
                            {
                                //digging is possible, but player will fall after digging
                                isFalling = true;
                                fallSpeedMultiplier = 1f;
                                animator.SetBool("isFalling", true);

                            }
                            else if (horizontal < 0)
                            {
                                //digging is not possible, but player can climb over/under
                                if (world.getTile(targetPos + new Vector3(0f, 1f, 0f)) == null)
                                {
                                    targetPos += new Vector3(0f, 1f, 0f);
                                }
                                else
                                {
                                    targetPos += new Vector3(0f, -1f, 0f);
                                }
                                
                                climbingDifficultyMultiplier = climbingDifficulty;

                                validDig = false;
                            }
                            else if (horizontal > 0)
                            {
                                //digging is not possible, but player can climb over/under
                                if (world.getTile(targetPos + new Vector3(0f, 1f, 0f)) == null)
                                {
                                    targetPos += new Vector3(0f, 1f, 0f);
                                }
                                else
                                {
                                    targetPos += new Vector3(0f, -1f, 0f);
                                }

                                climbingDifficultyMultiplier = climbingDifficulty;

                                validDig = false;
                            }
                            
                        }
                    }
                    else if (world.getTile(targetPos + new Vector3(0f, -1f, 0f)) == null || vertical != 0)
                    {
                        climbingDifficultyMultiplier = climbingDifficulty;
                    }

                    if (validDig)
                    {
                        if (world.getTile(targetPos) != null)
                        {
                            tileDifficultyMultiplier = world.getTile(targetPos).GetComponent<TileDifficulty>().difficulty * strengthMultiplier;
                            if (tileDifficultyMultiplier < 1f)
                            {
                                tileDifficultyMultiplier = 1f;
                            }
                        }
                        //Diggin Occurs
                        Destroy(moveTile);
                        isDestroyed = true;
                    }
                    

                }

                animator.SetFloat("speed", moveSpeed);

                moveCooldown = setMoveCooldown;
                
            }
            
        }
    }
    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

        lampy.xOffset *= -1;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        moveCooldown--;

        if (canMove)
        {
            targetPos = transform.position;
            Move();
        }
        if (moving)
        {
            if (transform.position == targetPos)
            {
                tileDifficultyMultiplier = 1f;
                climbingDifficultyMultiplier = 1f;

                if (!isFalling)
                {
                    animator.SetFloat("speed", 0f);
                    animator.SetBool("isJumping", false);
                    animator.SetBool("isFalling", false);

                    moving = false;
                    canMove = true;
                }
                else
                {
                    if (world.getTile(targetPos + new Vector3(0f, -1f, 0f)) != null)
                    {
                        isFalling = false;
                        fallSpeedMultiplier = 1f;
                    }
                    else
                    {
                        targetPos += new Vector3(0f, -1f, 0f);
                        fallSpeedMultiplier += 2f;
                    }
                }
                

            }

            transform.position = Vector2.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed * fallSpeedMultiplier / tileDifficultyMultiplier / climbingDifficultyMultiplier);
        }
        else
        {
            
        }
    }
}