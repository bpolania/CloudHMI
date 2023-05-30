PRAGMA page_size=4096;
CREATE TABLE "notes" (
   "ID" INTEGER PRIMARY KEY AUTOINCREMENT,
   "ERASED" INTEGER NOT NULL,
   "OPENED" INTEGER NOT NULL,
   "POINTNUM" INTEGER,
   "SCREENFILE" TEXT,
   "CONTENT" TEXT,
   "X" INTEGER,
   "Y" INTEGER,
   "OBJSHAPE" INTEGER,
   "OBJCOLOR" TEXT,
   "TEXTCOLOR" TEXT,
   "TEXTBGCOLOR" TEXT,
   "USER" TEXT,
   "TSCREATE" INTEGER,
   "TSERASE" INTEGER NOT NULL
   );
CREATE INDEX ind_pointnum on "notes" ( POINTNUM );
CREATE INDEX ind_erased on "notes" ( ERASED );
CREATE INDEX ind_screenfile on "notes" ( SCREENFILE );

