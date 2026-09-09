-> blacksmith_intro

=== blacksmith_intro ===
Blacksmith: You're not from around here. # id:intro # shot:portrait
Blacksmith: The northern road has been quiet lately. # id:road
* [Just passing through.]
    Player: I'm heading north. # id:north # shot:over_shoulder # target:Blacksmith
    -> friendly
* [None of your business.]
    Player: That's none of your concern!!. # id:refusal
    Player: Rawer.ss
    -> hostile

=== friendly ===
Blacksmith: Fair enough. You'll need a sound blade. # id:offer
Player: What happened to the road? # id:question
Troll: The bridge is gone. Take the old mill path. # id:warning
* [Thank the blacksmith.]
    Player: Thank you. I'll remember that. # id:thanks
    -> farewell
* [Ask about supplies.]
    Player: Can I buy supplies here? # id:supplies
    Blacksmith: My sister runs the store next door. # id:store
    -> farewell

=== hostile ===
Blacksmith: Watch your attitude. # id:attitude
Player: Sorry. It's been a long journey. # id:apology
-> farewell

=== farewell ===
Blacksmith: Safe travels, stranger. # id:farewell # shot:frame_share # target:Player
-> END
