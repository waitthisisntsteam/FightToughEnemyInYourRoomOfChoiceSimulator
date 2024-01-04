#include <Arduino.h>

int X;
int Y;
bool jump;

char binaryConverter(int x, int y, bool up)
{
  bool u = x < 350 || up;
  bool d = x > 650;
  bool l = y < 350;
  bool r = y > 675;

  char part1 = u << 3;
  char part2 = d << 2;
  char part3 = l << 1;
  char part4 = r;

  return part1 | part2 | part3 | part4;
}


void setup() 
{
  pinMode(2, INPUT);

  pinMode(A12, INPUT);
  pinMode(A13, INPUT);

  Serial.begin(150000);
}

void loop() 
{
  Y = analogRead(A12);
  X = analogRead(A13);

  if (digitalRead(2) > 0)
  {
    jump = true;
  }
  else
  {
    jump = false;
  }

  Serial.write((int)binaryConverter(X, Y, jump));
}