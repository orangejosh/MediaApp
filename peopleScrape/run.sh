#Gets the director from movieList.txt and removes empty lines, removes white spaces and prints the name. 
awk -F ';' '{print $3}' movieList.txt | awk 'NR%2==0'  | sed '/^[[:space:]]*$/d' >> p.txt 

#Gets each actor from movieList.txt and removes the empty lines, removes white spaces and prints the names.
awk -F ';' '{print $4}' movieList.txt | awk -F ',' '{print $1"\n" $2"\n" $3"\n" $4"\n"}' | sed -e 's/^[[:space:]]*//' | sed '/^[[:space:]]*$/d' >> p.txt

#Removes any duplicate names.
awk '!a[$0]++' p.txt >> peopleList.txt

#Removes the temporary txt file.
rm p.txt
