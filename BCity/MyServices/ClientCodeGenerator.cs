using BCity.Models;

namespace BCity.MyServices
{
    public class ClientCodeGenerator
    {
        private readonly MyDbContext _context;
        public ClientCodeGenerator(MyDbContext context)
        {
            _context = context;
        }


        public string GenerateClientCode(string name)
        {
            var codePrefix = GetCodePrefix(name); // get first three characters from Name

            var numberSuffix = GetNextAvailableNumber(codePrefix); // get next available number

            // concatente codePrefix + numberSuffix
            return $"{codePrefix}{numberSuffix:D3}";
        }


        // generate alphanumeric prefix
        private string GetCodePrefix(string name)
        {
            var nameParts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var prefix = "";

            if (nameParts.Length > 1)
            {
                // use first letter of each part
                prefix = string.Concat(nameParts.Select(part => part.Substring(0, 1).ToUpper()));
            }
            else {
                // use given name
                prefix = name.ToUpper();
            }

            // prefix must be 3 characters long
            var letter = "A";
            while (prefix.Length < 3)
            {
                prefix += letter;
                letter = "B";
            }

            return prefix.Length > 3 ? prefix.Substring(0, 3) : prefix;
        }


        // search db for next available number
        private int GetNextAvailableNumber(string codePrefix)
        {
            // get most recent client-code with the code-prefix
            var lastClientCode = _context.Clients
                .Where(c => c.ClientCode.StartsWith(codePrefix))
                .OrderByDescending(c => c.ClientCode)
                .Select(c => c.ClientCode)
                .FirstOrDefault();
            
            int nextNumber = 1;

            // if a client-code is found
            if (lastClientCode != null)
            {
                // get the numeric part
                var numericPart = lastClientCode.Substring(codePrefix.Length);
                if (int.TryParse(numericPart, out var currentNumber))
                {
                    nextNumber = currentNumber + 1; // ..increment
                }
            }

            return nextNumber;
        }
    }
}
