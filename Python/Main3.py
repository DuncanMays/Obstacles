import pygame, sys
from agent import agent
import random
from math import pi, cos, sin
from random import randint, random

#ALL THE FUNCTION AND stuff#####################################################################################

#sorts an array of agents using bubble sort based on their x position
def bubbleSort(list):
	#this is just a swap variable used to temporarily store one of two values while they're being swapped
	swap = 0

	for i in range(0, len(list)):
		for j in range(0, len(list)-i-1):
			#swaps two consevutive elements if the one on the left is bigger than the one on the right
			if(list[j].getX() < list[j+1].getX()):
				swap = list[j]
				list[j] = list[j+1]
				list[j+1] = swap

	return list

def bubbleSortS(list):
	#this is just a swap variable used to temporarily store one of two values while they're being swapped
	swap = 0

	for i in range(0, len(list)):
		for j in range(0, len(list)-i-1):
			#swaps two consevutive elements if the one on the left is bigger than the one on the right
			if(list[j][0] < list[j+1][0]):
				swap = list[j]
				list[j] = list[j+1]
				list[j+1] = swap

	return list

#simply draws a given agent on the screen
def draw(agent):
	pygame.draw.polygon(screen, agent.getColour(), [(x[0] + xShift, x[1]) for x in agent.getPointList()])
	pygame.draw.polygon(screen, (0,0,0), [(x[0] + xShift, x[1]) for x in agent.getPointList()], 2)

#this function produces a random sequence of DNA for use when the agents are first initialized, and basically nowhere else
def getRandomDNA():
	#creates DNA strand
	randDNA = [(255,0,0), pi/6,
			 [[0,0,0], [0,1,0] ],
			 [ [[1,0,0], [0,1,0], [0,0,1]], [[0,0,1], [0,-1,0], [1,0,0]], [[1,0,0], [0,1,0], [0,0,1]] ]]

	#sets the colour of the agent to something random
	randDNA[0] = (randint(0,255), randint(0,255), randint(0,255))
	#sets the spread of the agents eyes to something between pi/12 and pi/3
	randDNA[1] = pi/12 + random()*(pi/3 - pi/12)
	#this is the default state of each neuron, it is rare for a neuron to have a default value
	for i in range(0,3):
		for j in range(0,2):
			#10% chance for a neuron to have a default value
			if(random() > 0.90):
				randDNA[2][j][i] = 2*random() - 1
			else:
				randDNA[2][j][i] = 0

	#this is the meshnet between neurons, this is by far the most complicated piece, may be difficult to understand
	for i in range(0,3):
		for j in range(0,3):
			for k in range(0,3):
				rand = random()
				#20% chance of a positive connection between two neurons
				if(rand > 0.80):
					randDNA[3][i][j][k] = randint(5,100)/100
				#15% chance of a negative connection between neurons
				elif(rand > 0.65):
					randDNA[3][i][j][k] = -randint(5,100)/100
				else:
					randDNA[3][i][j][k] = 0

	return randDNA

#this function calculates how related two DNA strands are
#it does this by summing the inverses of the differences between each value in the DNA strand plus one
#it returns a float from 0 to 1, 1 being identical and 0 being completely different in every way.
#the colour held in the DNA is not taken into account
def getRelatedness(male, female):
	#does the thing for the sread of the eyes
	relatedness = 1/(1 + abs(male[1] - female[1]))
	#this is the max value that relatedness can take, depends on the number of ins and outs, and the number of neurons
	#this takes into account the spread of the eyes
	maxValue = 1
	#does the thing for the default value of each neuron in their brain
	for i in range(0,len(male[2])):
		for j in range(0,len(male[2][i])):
			relatedness += 1/(1 + abs(male[2][i][j] - female[2][i][j]))
			maxValue += 1
	#does the thing for the value of the meshnet connections
	for i in range(0,len(male[3])):
		for j in range(0,len(male[3][i])):
			for k in range(0,len(male[3][i][j])):
				relatedness += 1/(1 + abs(male[3][i][j][k] - female[3][i][j][k]))
				maxValue += 1

	return relatedness/maxValue

avgRelatedness = 0
#takes two DNA sequences and averages them
def mate(male, female):

	#each value get overwritten, so this just takes the structure since i cant figure out how to create it yet
	child = getRandomDNA()

	#averages the colours of the agents
	child[0] = [(male[0][i] + female[0][i])/2 for i in range(0,len(male[0]))]
	#averages the spread of the agents eyes
	child[1] = (female[1] + male[1])/2
	#averages the default value of each neuron in their brain
	for i in range(0,len(male[2])):
		for j in range(0,len(male[2][i])):
			child[2][i][j] = (male[2][i][j] + female[2][i][j])/2
	#averages the value of the meshnet connections
	for i in range(0,len(male[3])):
		for j in range(0,len(male[3][i])):
			for k in range(0,len(male[3][i][j])):
				child[3][i][j][k] = (male[3][i][j][k] + female[3][i][j][k])/2

	#changes the organism's colour so that it can be told apart from its parents
	colour = COLOURS[randint(0,len(COLOURS)-1)]

	#creates a random mutation in the child, allowing the AI to evolve
	for i in range(0,randint(1,2)):
		child = mutate(child)
		child[0] = [(14*child[0][i] + colour[i])/15 for i in range(0,3)]


	#if diversity falls, this dramatically increases the mutation rate to keep it high
	if(avgRelatedness > 0.85):
		for i in range(0,randint(1,25)):
			child = mutate(child)
			child[0] = [(14*child[0][i] + colour[i])/15 for i in range(0,3)]


	return child

#this function takes a string of DNA and alters it slightly and randomly, allowing agents to evolve
def mutate(inDNA):
	#this will be the DNA strand that gets passed out, we need to create a completely new strand
	outDNA = inDNA

	#this will be used to decide which feature of the DNA will be altered
	rand = randint(0,100)

	if(rand > 90):
		#10% chance of a new connection being formed between neurons
		#tries 10 times to find a pair of neurons without a connection
		for i in range(0,10):
			j = randint(0,2); k = randint(0,2); h = randint(0,2)
			if(outDNA[3][k][j][h] == 0):
				outDNA[3][k][j][h] = randint(-100, 100)/100
				break
	elif(rand > 80):
		#10% chance of a connection being severed between neurons
		#tries 10 times to find a pair of neurons with a connection
		for i in range(0,10):
			j = randint(0,2); k = randint(0,2); h = randint(0,2)
			if(outDNA[3][k][j][h]):
				outDNA[3][k][j][h] = 0
				break
	elif(rand > 60):
		#20% chance of an existing connection being made stronger
		#tries 10 times to find a pair of neurons with a connection
		for i in range(0,10):
			j = randint(0,2); k = randint(0,2); h = randint(0,2)
			if(outDNA[3][k][j][h]):
				outDNA[3][k][j][h] = outDNA[3][k][j][h]*(1 + randint(10,100)/100)
				break
	elif(rand > 40):
		#20% chance of an existing connection being made weaker
		#tries 10 times to find a pair of neurons with a connection
		for i in range(0,10):
			j = randint(0,2); k = randint(0,2); h = randint(0,2)
			if(outDNA[3][k][j][h]):
				outDNA[3][k][j][h] = outDNA[3][k][j][h]*(1 - randint(10,100)/100)
				#if the connection is less than 5%, it severs the connection completely
				if(outDNA[3][k][j][h] < 0.05):
					outDNA[3][k][j][h] = 0
				break
	elif(rand > 30):
		#10% chance of a neuron with default value being made stronger
		#tries 10 times to find a neuron with default value
		for i in range(0,10):
			j = randint(0,2); k = randint(0,1)
			if(outDNA[2][k][j]):
				outDNA[2][k][j] = outDNA[2][k][j]*(1 + randint(5,10)/100)
				break
	elif(rand > 20):
		#10% chance of a neuron with default value being made weaker
		#tries 10 times to find a neuron with default value
		for i in range(0,10):
			j = randint(0,2); k = randint(0,1)
			if(outDNA[2][k][j]):
				outDNA[2][k][j] = outDNA[2][k][j]*(1 - randint(5,20)/100)
				break
	elif(rand > 10):
		#10% chance of a neuron with no default value getting one
		#tries 10 times to find a neuron without default value
		for i in range(0,10):
			j = randint(0,2); k = randint(0,1)
			if(outDNA[2][k][j] == 0):
				outDNA[2][k][j] = randint(-30,30)/100
				break
	elif(rand > 5):
		#5% chance of the spread of the agents eyes increasing
		outDNA[1] = inDNA[1]*(1 + randint(3,6)/100)
	else:
		#5% chance of the spread of the agents eyes decreasing
		outDNA[1] = inDNA[1]*(1 - randint(3,6)/100)

	# #this part alters the DNA's colour just a bit so that its parent can be seen but it is still distinguishable
	# outDNA[0] = [(9*outDNA[0][i] + COLOURS[randint(0,len(COLOURS)-1)][i])/10 for i in range(0,3)]

	return outDNA
	

#this plots a neural network onto the screen so that it may be examined
def plotBrain(brain, neurons):
	#describe the position of the top left corner of the grid of neurons that's being drawn
	#and the distance between neurons in bot x and y
	xI = 40; yI = 20; xS = 85; yS = 35

	#draws the mesh
	#DNA[3] is the mesh array
	for i in range(0,3):
		for j in range(0,3):
			#DNA[3][j][i] is the 3-array which holds the mesh data for the node i,j
			for k in range(0,3):
				#if a connection exists at all, draw a line
				if(brain[i][j][k]):
					if(brain[i][j][k] > 0):
						#if it is a positive connection, the line is black
						pygame.draw.line(screen, (0,0,0), (xI + xS*(3-i), yI + yS*j), (xI + xS*(2-i), yI + yS*k), (int)(20*brain[i][j][k]))
					else:
						#if it is a negative connection, the line is red
						pygame.draw.line(screen, (255,0,0), (xI + xS*(3-i), yI + yS*j), (xI + xS*(2-i), yI + yS*k), -(int)(20*brain[i][j][k]))

	#draws the neurons on top of the mesh
	for i in range(0,4):
		for j in range(0,3):
			if(i == 0):
				#draws a blue square for the outputs instead of a circle
				pygame.draw.rect(screen, (0,0,255), (xI+xS*i - 13, yI+yS*j - 13, 26, 26))
				pygame.draw.rect(screen, (0,0,0), (xI+xS*i - 13, yI+yS*j - 13, 26, 26), 2)
			elif(i == 3):
				#pygame.draw.circle(screen, (0,0,0), (xI+xS*i, yI+yS*j), 15, 2)
				pygame.draw.polygon(screen, (0,255,0), ((xI+xS*i, yI+yS*j+15), (xI+xS*i+15, yI+yS*j), (xI+xS*i, yI+yS*j-15), (xI+xS*i-15, yI+yS*j)))
				pygame.draw.polygon(screen, (0,0,0), ((xI+xS*i, yI+yS*j+15), (xI+xS*i+15, yI+yS*j), (xI+xS*i, yI+yS*j-15), (xI+xS*i-15, yI+yS*j)), 2)
			else:
				pygame.draw.circle(screen, (255,255,255), (xI+xS*i, yI+yS*j), 15)
				pygame.draw.circle(screen, (0,0,0), (xI+xS*i, yI+yS*j), 15, 2)

	#writes the default value of each neuron on top of it
	# font = pygame.font.SysFont("comicsansms", 72)
	# text = font.render("hello world", True, (0, 128, 0))
	# screen.blit(text, (0, 0))
	# font = pygame.font.SysFont("comicsansms", 20)
	for i in range(1,3):
		for j in range(0,3):
			defVal = font.render((str)(truncate(neurons[i-1][j], 2)), True, (0,0,0))
			screen.blit(defVal, (xI + xS*(3-i) - defVal.get_width()/2, yI + yS*j - defVal.get_height()/2 + 1))

#this function controls how the relatedness of an organism effects its likelyhood for selection, used exclusively in the select function
#takes a number from 0 to 1 and outputs a number 0 to 1, maxed out at 1 for an organism of 95% similarity
#the function has a value of zero at 1, so identical organisms are never breeded. Yes, s stands for sexyness
#s = lambda r: (pow(r,5) - pow(r,50))/0.69684
s = lambda r: r
#selects a mate for an inputted organism
#becuase he really needs a girlfriend, im calling it Matty
#so the function will return a number from 0 to n-1 which is the index of the organism this one should mate with
#organisms can only mate up, that is they can only mate with a companion who has a higher x value than them
#besides that, the selection is partly random but also partly dependant on how related the organism is the the other,
#the more related the greater the chances, up to 95% similarity. It takes the index of the oranism in the array agents.
def select(index):
	matty = agents[index].getDNA()

	#this is a dual array holding both the index of each agent as well as its sexyness times its x value
	mates = [(agents[i].getX()*getRelatedness(matty, agents[i].getDNA()), i) for i in range(0, index)]

	#quicksorts mates based on sexyness times x value
	bubbleSortS(mates)

	#returns a random index in mates, preffering mates with a low index
	if(len(mates) == 0):
		return index
	else:
		return mates[randint(0,randint(0,len(mates)-1))][1]

#coppied off of stackoverflow, https://stackoverflow.com/questions/783897/truncating-floats-in-python
#please dont hate me
def truncate(f, n):
    '''Truncates/pads a float f to n decimal places without rounding'''
    s = '%.12f' % f
    i, p, d = s.partition('.')
    return '.'.join([i, (d+'0'*n)[:n]])

#ALL THE FUNCTIONS AND stuff#####################################################################################

#this is where the actual program begins

#initializes pygame
pygame.init()

#creates screen
#order of inputs is: screen dimensions, flags, colour
screen = pygame.display.set_mode((640,360),0,32)

#this variable is the number of times the loop below will execute every second, consequently, it is also the framerate of the screen
FPS = 24
#this object will pause every loop 1/FPS seconds so that the loop will execute FPS times a second
clock = pygame.time.Clock()

#NOT IMPORTANT stuff##########################################################################################

#this is the characteristic array of the AI, each elements in it holds the number of neurons in a layer,
#so if ca = [1,2,3] there would be 3 layers with 1 neuron, 2 neurons, and then 3 neurons in order from the
#inputs (eyes)
ca = [3, 3]
#this gets referenced a lot, so it's probably more efficient to predefine it rather than call len() every time
depth = len(ca)
#this is the numnber of inputs into the neural net
ins = 3
#this is the number of outputs from the neural net
outs = 3

#each organism is described using an array called DNA. It contains information which defines the agent, like the
#spread of its eyes, its colour, and all the nodes and connestions in its brain
#in order, the contents of the array are: the colour in RGB, the spread of its eyes, the default value of all its
#neurons, and the connections between those neurons.

#this is the number of agents, needs to be even
n = 100
#pre-allocates memory for the agent array
agents = [agent]*n
#this creates an array of agents with random DNA
for i in range(0,n):
	agents[i] = agent(getRandomDNA(), 20, 220)
	agents[i].DNA[0] = ((int)(255*random()), (int)(255*random()), (int)(255*random()))

# agents[0] = agent(topDNA, 20, 220)

#to make the screen appear to move left and right, this variable shifts all drawings
xShift = 0

#this is the boundaries of the arena, the order is left boundary, top, bottom, right
bounds = [0, 120, 320, 1600]

#the iteration number of the simulation
iterationNumber = 0
#keeps track of the generation
generation = 1

#this is the font that all the text will be displayed in, don't hate comic sans
font = pygame.font.SysFont("comicsansms", 20)
#this records the number of survivors each round
survivors = 0

#pretty colours
BLUE = [0,0,255]
RED = [255,0,0]
YELLOW = [255,255,0]
GREEN = [0,255,0]
PURPLE = [255,0,255]
ORANGE = [255,150,0]
COLOURS = [BLUE, RED, YELLOW, GREEN, PURPLE, ORANGE]

#defines the obstacle, 6,7,8,9 move around and so will be defined in the while loop below
obstacles = [[200, 220, 50, BLUE], [350, 130, 25, RED], [350, 220, 25, RED], [350, 310, 25, RED], 
	[500, 120, 75, YELLOW], [500, 320, 75, YELLOW], [650, 180, 30, PURPLE],
	[650, 260, 30, PURPLE], [750, 130, 30, PURPLE], [750, 220, 30, PURPLE], [750, 310, 30, PURPLE],
	[850, 180, 30, PURPLE], [850, 260, 30, PURPLE], [950, 130, 30, PURPLE], [950, 220, 30, PURPLE],
	[950, 310, 30, PURPLE], {}, {}, {}, {}, {}, {}, {}, {}]

#to prevent obstacles that move from being in the same place at the same time everytime, this variable adds some randomness
#to their position
randDisp = randint(0,100)

topDNA = getRandomDNA()

#NOT IMPORTANT stuff##########################################################################################

#game loop
while True:

	#IMPORTANT stuff THAT NEEDS TO GO AT THE BEGINNING
	#creates a white background
	#inputs are: (target, clolour, (x, y, width, height))
	pygame.draw.rect(screen, (255,255,255), (0,0,640,360))
	#IMPORTANT stuff THAT NEEDS TO GO AT THE BEGINNING

	#NOT IMPORTANT stuff##########################################################################################

	#this shifts the whole thing left or right
	if (pygame.key.get_pressed()[pygame.K_RIGHT]):
		xShift -= 20
	if (pygame.key.get_pressed()[pygame.K_LEFT]):
		xShift += 20

	#this increases and decreases the speed of the simulation
	if (pygame.key.get_pressed()[pygame.K_UP]):
		FPS += 1
	if (pygame.key.get_pressed()[pygame.K_DOWN]):
		FPS -= 1

	#this restarts the simulation every few seconds or so
	iterationNumber += 1
	if(iterationNumber%300 == 0):
		generation += 1
		randDisp = randint(0,100)
		iterationNumber = 0
		bubbleSort(agents)
		topDNA = agents[0].getDNA()
		survivors = 0

		#this breeds all the agents and does some other stuff
		for i in range(0, n):
			newDNA = mate(agents[i].getDNA(), agents[select(i)].getDNA())
			agents[n-i-1] = agent(newDNA, 20, 220)


			#updates the number of survivors on the screen
			if(agents[i].getX() >= bounds[3]):
				survivors += 1

		#calculates and displays the average relatedness of all the agents
		avgRelatedness = 0
		for i in range(0,n):
			avgRelatedness += getRelatedness(agents[randint(0,n-1)].getDNA(), agents[randint(0,n-1)].getDNA())
		avgRelatedness = avgRelatedness/n
		print(avgRelatedness)

	#This part draws the boundaries
	pygame.draw.line(screen, (0,0,0), (0,bounds[1]), (640,bounds[1]), 5)
	pygame.draw.line(screen, (0,0,0), (0,bounds[2]), (640,bounds[2]), 5)
	pygame.draw.line(screen, (0,0,0), (bounds[0] + xShift, bounds[1]), (bounds[0] + xShift, bounds[2]), 5)
	pygame.draw.line(screen, (255,0,0), (bounds[3] + xShift, bounds[1]), (bounds[3] + xShift, bounds[2]), 5)

	#the obstacles which move
	#the orange circles which move up and down
	obstacles[16] = [1055, 220 + int(90*sin(iterationNumber/25+randDisp)), 20, ORANGE]
	obstacles[17] = [1125, 220 + int(90*sin(iterationNumber/30-pi/2+randDisp)), 20, ORANGE]
	obstacles[18] = [1195, 220 + int(90*sin(iterationNumber/40-pi/3+randDisp)), 20, ORANGE]
	obstacles[19] = [1265, 220 + int(90*sin(iterationNumber/20+randDisp)), 20, ORANGE]
	#the green circles which turn around
	obstacles[20] = [1440+int(70*cos(iterationNumber/20+randDisp)), 220+int(70*sin(iterationNumber/20+randDisp)), 20, GREEN]
	obstacles[21] = [1440+int(70*cos(iterationNumber/20-pi/2+randDisp)), 220+int(70*sin(iterationNumber/20-pi/2+randDisp)), 20, GREEN]
	obstacles[22] = [1440+int(70*cos(iterationNumber/20+pi/2+randDisp)), 220+int(70*sin(iterationNumber/20+pi/2+randDisp)), 20, GREEN]
	obstacles[23] = [1440+int(70*cos(iterationNumber/20+pi+randDisp)), 220+int(70*sin(iterationNumber/20+pi+randDisp)), 20, GREEN]

	#draws the obstacles
	for i in obstacles:
		pygame.draw.circle(screen, i[3], (i[0] + xShift, i[1]), i[2])
		pygame.draw.circle(screen, (0,0,0), (i[0] + xShift, i[1]), i[2], 3)

	for i in agents:
		#checks if the agents are outside the boundaries of the arena
		i.checkDeathByBoundary(bounds)
		#gets each agent to check its eyes
		i.checkEyes(obstacles, bounds)
		#updates each agent with its velocity and all that
		i.update()
		#draws each agent on the screen
		draw(i)

	#this part draws the brain of the best agent, because it's cool
	plotBrain(topDNA[3], topDNA[2])
	#displays the spread of the top agents eyes
	spreadVal = font.render("eyes = " + (str)(truncate(topDNA[1], 3)), True, (0,0,0))
	screen.blit(spreadVal, (340, 25))
	#displays the generation 
	genVal = font.render("generation: " + (str)(generation), True, (0,0,0))
	screen.blit(genVal, (430, 25))
	#displays the from rate
	genVal = font.render("FPS: " + (str)(FPS), True, (0,0,0))
	screen.blit(genVal, (430, 50))
	#displays the number of survivors
	genVal = font.render("survival rate: " + (str)(survivors), True, (0,0,0))
	screen.blit(genVal, (530, 25))

	#NOT IMPORTANT stuff##########################################################################################

	#IMPORTANT stuff THAT NEEDS TO HAPPEN SO IT SHOULD PROBABLY GO AT THE END SO YOU WONT CHANGE IT
	#this block of code forces the program to end once the red "x" is clocked at the top left
	for event in pygame.event.get():
		if event.type == pygame.QUIT:
			pygame.quit()
			sys.exit()

	#continually updates the screen
	pygame.display.flip()

	clock.tick(FPS)
	#IMPORTANT stuff THAT NEEDS TO HAPPEN SO IT SHOULD PROBABLY GO AT THE END SO YOU WONT CHANGE IT

	