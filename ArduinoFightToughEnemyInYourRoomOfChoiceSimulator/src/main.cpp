#include <Arduino.h>
#include <Adafruit_NeoPixel.h>

int X;
int Y;
bool jump;

int numOfLeds = 12;
Adafruit_NeoPixel strip(numOfLeds, 7, NEO_GBR + NEO_KHZ800);

double findAngle(int led)
{
  return 15 + led * 30;
}

double findAngleDifference(double targetAngle, double mainAngle)
{
  double maxAngle = max(targetAngle, mainAngle);
  double minAngle = min(targetAngle, mainAngle);

  int differece1 = abs(maxAngle - minAngle);
  int differece2 = abs(maxAngle - 360 - minAngle);

  return min(differece1, differece2);
}

void draw(int led, double joystickAngle)
{
  int angleOfLed = findAngle(led);

  double difference = findAngleDifference(angleOfLed, joystickAngle);
  double brightness = 1.0 - (difference / 180);
  brightness *= 0.8;
  if (brightness < 0.1)
  {
    brightness = 0;
  }

  strip.setPixelColor(led, brightness * 153, brightness * 204, brightness * 255);
}

uint8_t Red(uint32_t color)
{
  return (color >> 16) & 0xFF;
}

uint8_t Green(uint32_t color)
{
  return (color >> 8) & 0xFF;
}

uint8_t Blue(uint32_t color)
{
  return color & 0xFF;
}

uint32_t DimColor(uint32_t color)
{
  uint32_t dimColor = strip.Color(Red(color) >> 1, Green(color) >> 1, Blue(color) >> 1);
  return dimColor;
}

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

  strip.setBrightness(20);

  strip.begin();
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

  X -= 512;
  Y -= 512;

  double angle = atan2(X, Y) + PI;
  int angleInDegrees = angle * (180 / PI);

  bool xDeadzone = abs(X) < 75;
  bool yDeadzone = abs(Y) < 75;

  if (xDeadzone && yDeadzone) {
    // todo only delay if any color is nonzero
    delay(50);
    for (int i = 0; i < numOfLeds; i++)
    {
      auto d = DimColor(strip.getPixelColor(i));
      strip.setPixelColor(i, d);
    }
  }
  else
  {
    strip.clear();
    for (int i = 0; i < numOfLeds; i++)
    {
      draw(i, angleInDegrees);
    }
  }

  if (jump)
  {
    if (xDeadzone && yDeadzone) {
      strip.clear();
    }
    for (int i = 0; i <= 5; i++)
    {
      draw(i, 90);
    }
  }

  strip.show();
}