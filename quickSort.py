from random import randint

def insertionSort(array):
    for i in range(1, len(array)):
        val = array[i]
        j = i - 1
        while (j >= 0) and (array[j] < val):
            array[j+1] = array[j]
            j = j - 1
        array[j+1] = val

#takes an array, and the index of a pivot element, and moves all elements greater than the pivot element to its left,
#and all elements less than or equal to the pivot elements to its right, and returns the new index of the pivot element
def partition(array, left, right):
	i = left;
	j = right - 1;
	pivot_index = left + int((right - left) / 2);
	pivot = array[pivot_index];
	temp = array[right]
	array[right] = array[pivot_index]
	array[pivot_index] = temp
	while (i < j):
		while (array[i] < pivot):
			i += 1
		while (array[j] > pivot):
			j -= 1
		if (i <= j):
			temp = array[i]
			array[i] = array[j]
			array[j] = temp
			i += 1
			j -= 1
	array[right] = array[i]
	array[i] = pivot
	return i

#left is the index of the first element, right is the index of the last element
def quickSort(array, left, right):
	if(right - left > 2):
		pivot = partition(array, left, right)
		quickSort(a, left, pivot - 1);
		quickSort(a, pivot + 1, right);
	else:
		array = insertionSort(array)

n = 100

a = [int]*n

for i in range(0,n):
	a[i] = randint(0,n)

print(a)
quickSort(a, 0, len(a)-1)
print(a)