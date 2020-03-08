variable player-health
variable player-agility
variable player-strength

variable monsters
variable monster-builders

12 constant monster-num

: init-player
  30 player-health   !
  30 player-agility  !
  30 player-strength ! ;

: player-dead? ( -- f ) player-health @ 0 <= ;

: player-attacks-per-round ( -- n )
  player-agility @
  15 /                          ( no need for truncating integer operations )
  1+ ;

: show-player
  cr
  ." You are a valiant knight with a health of "
  ( Note that ? or @ . is printing a space afterwards. Need to figure out how to
    circunvent this. )
  player-health ?
  ." , an agility of "
  player-agility ?
  ." , and a strength of "
  player-strength ?
  ." ." ;

: pick-monster ;                ( nyi )

: player-attack
  cr ." Attack style: [s]tab [d]ouble swing [r]oundhouse:"
  key case
    115 of                      ( Stab )
      pick-monster
    endof
    100 of                      ( Double Swing )
      pick-monster
    endof
  endcase
;

: init-monsters ;               ( nyi )
: show-monsters ;               ( nyi )
: monsters-dead? true ;         ( nyi )

: game-loop
  recursive
  player-dead? monsters-dead? or if
    exit
  then
  show-player
  player-attacks-per-round -1 do
    monsters-dead? not if
      show-monsters
      player-attack
    then
  loop
  ( TODO Make monsters attack. Basically need to loop through the array of
    monsters, and if the monster is not dead, then make it attack the player. )
  game-loop
;

: .player-dead
  player-dead? if
    cr ." You have been killed. Game over." cr
  then ;

: .monsters-dead
  monsters-dead? if
    cr ." Congratulations! You have vanquised all of your foes." cr
  then ;

: end-game   .player-dead .monsters-dead  ;
: init-game  init-monsters init-player    ;
: orc-battle init-game game-loop end-game ;
