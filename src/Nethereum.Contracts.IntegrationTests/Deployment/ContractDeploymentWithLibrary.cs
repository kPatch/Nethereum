using Nethereum.Hex.HexTypes;
using Nethereum.XUnitEthereumClients;
using Xunit;

namespace Nethereum.Contracts.IntegrationTests.Deployment
{
    [Collection(EthereumClientIntegrationFixture.ETHEREUM_CLIENT_COLLECTION_DEFAULT)]
    public class ContractDeploymentWithLibrary
    {
        private readonly EthereumClientIntegrationFixture _ethereumClientIntegrationFixture;
        private const string _placeholderMarker = "__$";

        public ContractDeploymentWithLibrary(EthereumClientIntegrationFixture ethereumClientIntegrationFixture)
        {
            _ethereumClientIntegrationFixture = ethereumClientIntegrationFixture;
        }

        [Fact]
        public void ShouldFailToDeployContractWithUnlinkedLibrary()
        {
            // Check bytecode is unlinked
            var contractByteCode = "608060405234801561001057600080fd5b5061025e806100206000396000f3fe608060405234801561001057600080fd5b506004361061002b5760003560e01c806379a7b63414610030575b600080fd5b6100386100ad565b6040805160208082528351818301528351919283929083019185019080838360005b8381101561007257818101518382015260200161005a565b50505050905090810190601f16801561009f5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6060600073__$4f6e1f7166b61d394a3a463d15dc4917b6$__63cfb519286040518163ffffffff1660e01b8152600401808060200182810382526002815260200180600160f11b611a190281525060200191505060206040518083038186803b15801561011957600080fd5b505af415801561012d573d6000803e3d6000fd5b505050506040513d602081101561014357600080fd5b505160408051600160e01b638e5fc30b02815260048101839052600060248201819052915192935073__$4f6e1f7166b61d394a3a463d15dc4917b6$__92638e5fc30b92604480840193919291829003018186803b1580156101a457600080fd5b505af41580156101b8573d6000803e3d6000fd5b505050506040513d6000823e601f3d908101601f1916820160405260208110156101e157600080fd5b8101908080516401000000008111156101f957600080fd5b8201602081018481111561020c57600080fd5b815164010000000081118282018710171561022657600080fd5b5090969550505050505056fea165627a7a723058207c1a7d9742f9ca8ee9b88baeb793a3ca5a975d11d8c0b721c5421bcef3ad66220029";
            Assert.Contains(_placeholderMarker, contractByteCode);

            // Deploy unlinked contract, which should throw exception
            var senderAddress = AccountFactory.Address;
            var web3 = _ethereumClientIntegrationFixture.GetWeb3();
            var ex = Assert.ThrowsAsync<System.FormatException>(
                async () => await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(contractByteCode,
                senderAddress, new HexBigInteger(900000), null, null, null)).Result;
            Assert.Contains($"could not be converted to byte array", ex.Message);
        }

        [Fact]
        public async void ShouldDeployContractWithLibrary()
        {
            // Deploy library
            var libraryByteCode = "6103fd610026600b82828239805160001a60731461001957fe5b30600052607381538281f3fe73000000000000000000000000000000000000000030146080604052600436106100405760003560e01c80638e5fc30b14610045578063cfb519281461006e575b600080fd5b610058610053366004610241565b61008e565b604051610065919061030d565b60405180910390f35b61008161007c36600461027b565b6101b8565b60405161006591906102ff565b6040805160208082528183019092526060918291906020820181803883390190505090506000805b602081101561010c576008810260020a86026001600160f81b031981161561010357808484815181106100e557fe5b60200101906001600160f81b031916908160001a9053506001909201915b506001016100b6565b5060008185118061011b575084155b1561012757508061012e565b5060001984015b6060816040519080825280601f01601f19166020018201604052801561015b576020820181803883390190505b50905060005b828110156101ab5784818151811061017557fe5b602001015160f81c60f81b82828151811061018c57fe5b60200101906001600160f81b031916908160001a905350600101610161565b5093505050505b92915050565b805160009082906101cd5750600090506101d5565b505060208101515b919050565b60006101e6823561037a565b9392505050565b600082601f8301126101fe57600080fd5b813561021161020c82610345565b61031e565b9150808252602083016020830185838301111561022d57600080fd5b61023883828461037d565b50505092915050565b6000806040838503121561025457600080fd5b600061026085856101da565b9250506020610271858286016101da565b9150509250929050565b60006020828403121561028d57600080fd5b813567ffffffffffffffff8111156102a457600080fd5b6102b0848285016101ed565b949350505050565b6102c18161037a565b82525050565b60006102d28261036d565b6102dc8185610371565b93506102ec818560208601610389565b6102f5816103b9565b9093019392505050565b602081016101b282846102b8565b602080825281016101e681846102c7565b60405181810167ffffffffffffffff8111828210171561033d57600080fd5b604052919050565b600067ffffffffffffffff82111561035c57600080fd5b506020601f91909101601f19160190565b5190565b90815260200190565b90565b82818337506000910152565b60005b838110156103a457818101518382015260200161038c565b838111156103b3576000848401525b50505050565b601f01601f19169056fea265627a7a72305820cc4eeddee6d0c5f899bd43d1fabdd2f1c121f7e4f4be0ec72d71625a1447872f6c6578706572696d656e74616cf50037";
            var senderAddress = AccountFactory.Address;
            var web3 = _ethereumClientIntegrationFixture.GetWeb3();
            var libraryReceipt = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(libraryByteCode,
                senderAddress, new HexBigInteger(900000), null, null, null);
            Assert.Equal(libraryReceipt.Status, new HexBigInteger(1));


            // Prepare mapping for linking the library into the main contract
            var libFullPath = "c:/Users/Kevin/Documents/GitHub/Nethereum_SapIntegrationPoC/PurchaseContracts/contracts/StringLib.sol";
            var libName = "StringLib";
            var libraryMapping = ByteCodeLibrary.CreateFromPath(libFullPath, libName, libraryReceipt.ContractAddress);
            var libraryExpectedPlaceholderKey = "4f6e1f7166b61d394a3a463d15dc4917b6";
            Assert.Equal(libraryExpectedPlaceholderKey, libraryMapping.PlaceholderKey);


            // Link main contract byte code with the library, in preparation for deployment
            var contractByteCode = "608060405234801561001057600080fd5b5061025e806100206000396000f3fe608060405234801561001057600080fd5b506004361061002b5760003560e01c806379a7b63414610030575b600080fd5b6100386100ad565b6040805160208082528351818301528351919283929083019185019080838360005b8381101561007257818101518382015260200161005a565b50505050905090810190601f16801561009f5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6060600073__$4f6e1f7166b61d394a3a463d15dc4917b6$__63cfb519286040518163ffffffff1660e01b8152600401808060200182810382526002815260200180600160f11b611a190281525060200191505060206040518083038186803b15801561011957600080fd5b505af415801561012d573d6000803e3d6000fd5b505050506040513d602081101561014357600080fd5b505160408051600160e01b638e5fc30b02815260048101839052600060248201819052915192935073__$4f6e1f7166b61d394a3a463d15dc4917b6$__92638e5fc30b92604480840193919291829003018186803b1580156101a457600080fd5b505af41580156101b8573d6000803e3d6000fd5b505050506040513d6000823e601f3d908101601f1916820160405260208110156101e157600080fd5b8101908080516401000000008111156101f957600080fd5b8201602081018481111561020c57600080fd5b815164010000000081118282018710171561022657600080fd5b5090969550505050505056fea165627a7a723058207c1a7d9742f9ca8ee9b88baeb793a3ca5a975d11d8c0b721c5421bcef3ad66220029";
            var libraryMappings = new ByteCodeLibrary[] { libraryMapping };
            var libraryLinker = new ByteCodeLibraryLinker();
            var contractByteCodeLinked = libraryLinker.LinkByteCode(contractByteCode, libraryMappings);
            // should be no link placeholders now
            Assert.DoesNotContain(_placeholderMarker, contractByteCodeLinked);


            // Deploy linked contract
            var contractReceipt = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(contractByteCodeLinked,
                senderAddress, new HexBigInteger(900000), null, null, null);
            Assert.Equal(contractReceipt.Status, new HexBigInteger(1));
        }
    }
}
/* LIBRARY
pragma solidity ^0.5.3;

library StringLib
{
    /// @dev Set truncateToLength to <= 0 to take max bytes available
    function bytes32ToString(bytes32 x, uint truncateToLength) public pure returns (string memory) 
    {
        bytes memory bytesString = new bytes(32);
        uint charCount = 0;
        
        for (uint j = 0; j < 32; j++) 
        {
            byte char = byte(bytes32(uint(x) * 2 ** (8 * j)));
            if (char != 0) 
            {
                bytesString[charCount] = char;
                charCount++;
            }
        }
        
        uint finalLength = 0;
        if (truncateToLength > charCount || truncateToLength <= 0)
        {
            finalLength = charCount;
        }
        else
        {
            finalLength = truncateToLength - 1;
        }
        
        bytes memory bytesStringTrimmed = new bytes(finalLength);
        for (uint j = 0; j < finalLength; j++) 
        {
            bytesStringTrimmed[j] = bytesString[j];
        }
        return string(bytesStringTrimmed);
    }
    
    /// @dev Pads shorter strings with 0, truncates longer strings to length 32
    function stringToBytes32(string memory source) public pure returns (bytes32 result) 
    {
        bytes memory tempEmptyStringTest = bytes(source);
        if (tempEmptyStringTest.length == 0) 
        {
            return 0x0;
        }

        assembly 
        {
            result := mload(add(source, 32))
        }
    }
}
 */

/* MAIN CONTRACT
pragma solidity ^0.5.3;

import "./StringLib.sol";

contract StringUser
{    
    /// @dev should return string "42"
    function testLibraryUse() public pure returns (string memory result)
    {
        // Converts 42 to bytes32 and back to string
        bytes32 b = StringLib.stringToBytes32("42");
        result = StringLib.bytes32ToString(b, 0);
    }
}
 */
