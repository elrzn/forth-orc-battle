variable player-health
variable player-agility
variable player-strength

variable monsters

12 constant monster-num

: randval ( n -- n' ) choose 1+ ;

struct
  cell%  field monster-health
  \ Keep an address of the word that will be used to display the struct. The
  \ idea is that each constructor shall set its own display function. This can
  \ be called later on with monster-show.
  cell%  field monster-show-addr
  dcell% field monster-name
end-struct monster%

: monster-name!          ( c-addr u addr -- ) monster-name 2! ;
: monster-name@          ( addr -- c-addr u ) monster-name 2@ ;
: monster-show           ( addr -- )          dup monster-show-addr @ execute ;
: .monster-name          ( addr -- )          monster-name@ type ;
: .monster               ( addr -- )          ." A fierce " .monster-name ;
: monster-default-health ( addr -- )          10 randval swap monster-health ! ;
: monster-default-name   ( addr -- )          s" Monster" rot monster-name! ;
: monster-default-show   ( addr -- )          ['] .monster swap monster-show-addr ! ;

: make-monster ( -- monster )
  monster% %allot
  dup monster-default-health
  dup monster-default-name
  dup monster-default-show ;

monster%
  cell% field wicked-orc-club-level
end-struct wicked-orc%

: .wicked-orc ( addr -- )
  ." A wicked orc with a level " wicked-orc-club-level ? ." club" ;

: make-wicked-orc ( -- addr )
  wicked-orc% %allot
  dup monster-default-health
  dup s" Wicked Orc" rot monster-name!
  dup ['] .wicked-orc swap monster-show-addr !
  8 randval over wicked-orc-club-level ! ;

\ Keep track of builders to aid random creation of monsters.
create monster-builders ' make-wicked-orc ,

: init-player
  30 player-health   !
  30 player-agility  !
  30 player-strength ! ;

: player-dead? ( -- f ) player-health @ 0<= ;

: monster-dead? ( monster -- f ) monster-health @ 0<= ;

: player-attacks-per-round ( -- n )
  player-agility @
  15 /                          \ no need for truncating integer operations
  1+ ;

: .player
  cr
  ." You are a valiant knight with a health of "
  \ Note that ? or @ . is printing a space afterwards. Need to figure out how to
  \ circunvent this.
  player-health ?
  ." , an agility of "
  player-agility ?
  ." , and a strength of "
  player-strength ?
  ." ." ;

: monster-take-damage ( monster damage -- )
  tuck
  over monster-health @
  swap -
  swap monster-health ! ;

: monster-hit ( monster damage -- )
  2dup monster-take-damage
  cr
  monster-dead? if
    drop
    ." You killed the " .monster-name ." !"
  else
    swap
    ." You hit the " .monster-name ." , knocking off " . ." health points!"
  then ;

: pick-monster   ( -- monster ) ;  \ nyi 178
: random-monster ( -- monster ) ;  \ nyi 177

: init-monsters ;               \ nyi 178
: show-monsters ;               \ nyi
: monsters-dead? true ;         \ nyi
: monsters-alive? monsters-dead? not ;

: player-stab-attack
  pick-monster                    ( monster )
  player-strength @ 2/ randval 2+ ( monster damage )
  monster-hit ;

: player-double-swing-attack
  pick-monster                  ( monster )
  player-strength @ 6 / randval ( monster damage )
  cr ." Your double swing has a strength of " dup .
  tuck                          ( damage monster damage )
  monster-hit                   ( damage )
  monsters-alive? if
    pick-monster swap           ( monster damage )
    monster-hit
  else
    drop
  then ;

: player-attack-other
  player-strength @ 3 / 1+      ( times )
  -1 do
    monsters-alive? if
      random-monster 1 monster-hit
    then
  loop ;

: player-attack
  cr ." Attack style: [s]tab [d]ouble swing [r]oundhouse:"
  key case
    's' of player-stab-attack         endof
    'd' of player-double-swing-attack endof
           player-attack-other
  endcase ;

: game-loop
  recursive
  player-dead? monsters-dead? or if
    exit
  then
  .player
  player-attacks-per-round -1 do
    monsters-alive? if
      show-monsters
      player-attack
    then
  loop
  \ TODO Make monsters attack. Basically need to loop through the array of
  \ monsters, and if the monster is not dead, then make it attack the player.
  game-loop ;

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
