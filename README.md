# BBB
**Group project’s topic:** Board-games library and management system  
### Team 20
### Student: Simaresi Paraskevi-Dimitra

## Deliverables
1. Source code (root directory)
1. Report [source](/Deliverables/Simaresi-Report-IndividualExtension.pdf)
1. Demonstration Video [source](/Deliverables/video-demo.mp4) (duration 2min)

## Individual Extension

**Topic:** REST API  
**Motivation:**
Future development of mobile app
Integration with University’s Outlook system (our indented user is the SDU community so we have to authenticate them through their SDU account)
My intention to learn in practice about REST API (and swagger)  

**Use case:**  
User makes GET request to `/api/games?status=available&tag=strategy` to display available games with strategy tag  
Admin manages games at `/api/admin/games`  

## Application Initialization

1. Navigate to root directory

1. Start application (for production):
```bush
dotnet run
```
1. Start application (for developments):
```bush
dotnet watch run
```

   The API will be available at:
   - HTTP: `http://localhost:5164`
   - Swagger: `http://localhost:5164/swagger`

## Users credentials

You are welcome to register a new user.  
But for your convenience we provide the following already created users:

**Admin**

1.  > admin  
        admin

1. > mario  
    mariopass

**Basic user**

1. >luigi  
    luigipass

1. > user  
    user


## List of Requirements and Use Cases
### Functional Requirements:
1. 2 users: borrower (B) and admin (A) (boardgame members)
1. B can see the board games list and availability; Can press to borrow (reserve)/go to the waiting list.
1. A can see the board games list and add new/remove board games
1. A is responsible for making board games literally un-/available
1. Users can search board games and use filters (title, category, availability, number of players)
1. Log in view - use uni e-mail to create a user, choose an unrelated password
1. Confirmation email upon borrowing board games for both A and B
1. Reminder emails to return board games
1. Limit to how many emails can be sent/board games can be reserved by a user
1. B view with borrowed board games, and when to return them or waiting list board games
1. Account editing options, e.g., name, etc.
1. The system should validate that the new user’s email and username are unique in the Database  

```
____________________________________  
|                                   |  
| |Photo| description (AVAILABLE)   |  
|___________________________________|  

    GUI layout for board games
```

### Non-functional Requirements:
1. Scalable system, i.e., can easily add new borrowers, new games to the database
1. Capacity - min. 100 board games, min. 100 borrower-users(B)
1. Database stores/restores consistent data
1. Security/authentication - safely storing passwords (hash, encrypt)
1. Non-glitchy, responsive, and intuitive UI
1. Graceful error handling
1. Forgiving search algorithm (fuzzy search)

### User case:
#### Boardgame Club enjoyers
1. A visitor of the website can browse the available collection.
1. A user can sign up with their account credentials.
1. Borrower
    1. A borrower can search for an entry using filters
    1. A borrower can preview availability and reserve a board game
    1. A borrower can see the history of reserved items.
    1. The user can extend the period of the borrowed item.
1. Admin
    1. An admin is able to change the status of a board game as un-/available.
    1. The admin can see all reserved and borrowed board games.
    1. The admin can extend the borrowing period.
    1. The admin can edit existing entries, add new ones, or remove retired ones.
    1. The admin can delete a borrow-user

